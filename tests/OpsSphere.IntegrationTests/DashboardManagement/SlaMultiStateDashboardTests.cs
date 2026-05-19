using System.Net;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using OpsSphere.Domain.Enums;
using OpsSphere.Infrastructure.Persistence;
using OpsSphere.Infrastructure.Persistence.SeedData;
using OpsSphere.IntegrationTests.TestInfrastructure;

namespace OpsSphere.IntegrationTests.DashboardManagement;

public sealed class SlaMultiStateDashboardTests
{
    private const string AgentEmail = "agent.novabank@opssphere.local";
    private const string SupervisorEmail = "supervisor.novabank@opssphere.local";
    private const int SeededNovaBankTicketCount = 6;

    [Fact]
    public async Task SlaSummary_WithAllFourStates_ReturnsCorrectCounts()
    {
        await using var factory = await OpsSphereSqliteFactory.CreateAsync();
        await SetupAllFourSlaStatesAsync(factory);
        var agent = await factory.CreateAuthenticatedClientAsync(AgentEmail);

        var response = await agent.GetAsync("/api/sla/summary");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var summary = await OpsSphereSqliteFactory.ReadDataAsync<SlaSummaryResponse>(response);

        Assert.Equal(2, summary.WithinSlaCount);
        Assert.Equal(1, summary.AtRiskCount);
        Assert.Equal(1, summary.BreachedCount);
        Assert.True(summary.CompletedCount >= 2);
    }

    [Fact]
    public async Task SlaSummary_WithoutAuth_Returns401()
    {
        await using var factory = await OpsSphereSqliteFactory.CreateAsync();

        var response = await factory.CreateClient().GetAsync("/api/sla/summary");

        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    [Fact]
    public async Task SlaSummary_IsScopedByCampaign_ForAgent()
    {
        await using var factory = await OpsSphereSqliteFactory.CreateAsync();
        await AddOutOfScopeBreachedTicketAsync(factory);
        var agent = await factory.CreateAuthenticatedClientAsync(AgentEmail);

        var response = await agent.GetAsync("/api/sla/summary");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var summary = await OpsSphereSqliteFactory.ReadDataAsync<SlaSummaryResponse>(response);
        Assert.Equal(2, summary.BreachedCount);
    }

    [Fact]
    public async Task OperationalDashboard_WithCampaignFilter_IntersectsScope()
    {
        await using var factory = await OpsSphereSqliteFactory.CreateAsync();
        var supervisor = await factory.CreateAuthenticatedClientAsync(SupervisorEmail);

        var response = await supervisor.GetAsync($"/api/dashboard/operational?campaignId={SeedIds.Campaigns.NovaBankCreditCard}");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var dashboard = await OpsSphereSqliteFactory.ReadDataAsync<OperationalDashboardResponse>(response);
        Assert.All(dashboard.TicketsByCampaign, item => Assert.Equal(SeedIds.Campaigns.NovaBankCreditCard, item.EntityId));
    }

    [Fact]
    public async Task OperationalDashboard_WithStatusFilter_ReturnsFilteredCounts()
    {
        await using var factory = await OpsSphereSqliteFactory.CreateAsync();
        var agent = await factory.CreateAuthenticatedClientAsync(AgentEmail);

        var response = await agent.GetAsync("/api/dashboard/operational?status=Open");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var dashboard = await OpsSphereSqliteFactory.ReadDataAsync<OperationalDashboardResponse>(response);
        Assert.Equal(1, dashboard.TotalTicketCount);
        Assert.Equal(1, dashboard.OpenTicketCount);
        Assert.Equal(0, dashboard.AssignedTicketCount);
    }

    [Fact]
    public async Task OperationalDashboard_WithDateRangeFilter_ReturnsCorrectCounts()
    {
        await using var factory = await OpsSphereSqliteFactory.CreateAsync();
        var agent = await factory.CreateAuthenticatedClientAsync(AgentEmail);

        var dateFrom = DateTime.UtcNow.AddDays(-365).ToString("yyyy-MM-ddTHH:mm:ssZ");
        var dateTo = DateTime.UtcNow.AddDays(1).ToString("yyyy-MM-ddTHH:mm:ssZ");

        var response = await agent.GetAsync($"/api/dashboard/operational?dateFrom={dateFrom}&dateTo={dateTo}");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var dashboard = await OpsSphereSqliteFactory.ReadDataAsync<OperationalDashboardResponse>(response);
        Assert.Equal(SeededNovaBankTicketCount, dashboard.TotalTicketCount);
    }

    private static async Task SetupAllFourSlaStatesAsync(OpsSphereSqliteFactory factory)
    {
        using var scope = factory.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<OpsSphereDbContext>();
        var now = DateTime.UtcNow;

        var rows = await db.TicketSlaStates
            .Include(s => s.Ticket)
            .Where(s => s.Ticket.CampaignId == SeedIds.Campaigns.NovaBankCreditCard && !s.Ticket.IsDeleted)
            .ToListAsync();

        var openRow = rows.FirstOrDefault(s => s.Ticket.Status == TicketStatus.Open);
        var assignedRow = rows.FirstOrDefault(s => s.Ticket.Status == TicketStatus.Assigned);
        var inProgressRow = rows.FirstOrDefault(s => s.Ticket.Status == TicketStatus.InProgress);
        var escalatedRow = rows.FirstOrDefault(s => s.Ticket.Status == TicketStatus.Escalated);

        if (openRow is not null)
        {
            openRow.StartedAt = now.AddHours(-1);
            openRow.DueAt = now.AddHours(23);
            openRow.AtRiskThresholdPercent = 80;
            openRow.State = SlaState.WithinSla.ToString();
        }

        if (assignedRow is not null)
        {
            assignedRow.StartedAt = now.AddHours(-2);
            assignedRow.DueAt = now.AddHours(22);
            assignedRow.AtRiskThresholdPercent = 80;
            assignedRow.State = SlaState.WithinSla.ToString();
        }

        if (inProgressRow is not null)
        {
            inProgressRow.StartedAt = now.AddHours(-22);
            inProgressRow.DueAt = now.AddHours(2);
            inProgressRow.AtRiskThresholdPercent = 80;
            inProgressRow.State = SlaState.WithinSla.ToString();
        }

        if (escalatedRow is not null)
        {
            escalatedRow.StartedAt = now.AddHours(-25);
            escalatedRow.DueAt = now.AddHours(-1);
            escalatedRow.AtRiskThresholdPercent = 80;
            escalatedRow.State = SlaState.WithinSla.ToString();
        }

        await db.SaveChangesAsync();
    }

    private static async Task AddOutOfScopeBreachedTicketAsync(OpsSphereSqliteFactory factory)
    {
        using var scope = factory.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<OpsSphereDbContext>();
        var campaign = await db.Campaigns
            .AsNoTracking()
            .Where(c => c.Id == SeedIds.Campaigns.StreamlyCreator)
            .Select(c => new { c.CountryId, c.Country.RegionId })
            .SingleAsync();

        var now = DateTime.UtcNow;
        var ticketId = Guid.NewGuid();
        db.Tickets.Add(new Domain.Entities.Ticket
        {
            Id = ticketId,
            TicketNumber = $"OPS-20990104-{System.Random.Shared.Next(1, 999999):D6}",
            CustomerId = SeedIds.Customers.StreamlyCustomer1,
            RegionId = campaign.RegionId,
            CountryId = campaign.CountryId,
            AccountId = SeedIds.Accounts.Streamly,
            CampaignId = SeedIds.Campaigns.StreamlyCreator,
            CreatedByUserId = SeedIds.Users.SupervisorNovabank,
            Category = "Support",
            Priority = TicketPriority.Normal,
            Status = TicketStatus.Open,
            Subject = "Out-of-scope SLA test ticket",
            Description = "Should not appear in agent SLA summary",
            SlaState = SlaState.WithinSla,
            SlaDueAt = now.AddHours(-5),
            IsEscalated = false,
            IsDeleted = false,
            CreatedAt = now
        });
        db.TicketSlaStates.Add(new Domain.Entities.TicketSlaState
        {
            Id = Guid.NewGuid(),
            TicketId = ticketId,
            StartedAt = now.AddHours(-10),
            DueAt = now.AddHours(-5),
            AtRiskThresholdPercent = 80,
            State = SlaState.WithinSla.ToString()
        });
        await db.SaveChangesAsync();
    }

    private sealed record SlaSummaryResponse(int WithinSlaCount, int AtRiskCount, int BreachedCount, int CompletedCount);
    private sealed record OperationalDashboardResponse(
        DateTime GeneratedAtUtc,
        int TotalTicketCount,
        int OpenTicketCount,
        int AssignedTicketCount,
        int EscalatedTicketCount,
        int BreachedTicketCount,
        int AtRiskTicketCount,
        IReadOnlyList<DashboardGroupItemResponse> TicketsByStatus,
        IReadOnlyList<DashboardGroupItemResponse> TicketsByPriority,
        IReadOnlyList<DashboardGroupItemResponse> TicketsBySlaState,
        IReadOnlyList<DashboardEntityGroupItemResponse> TicketsByAccount,
        IReadOnlyList<DashboardEntityGroupItemResponse> TicketsByCampaign,
        IReadOnlyList<DashboardUserGroupItemResponse> TicketsByAssignedAgent,
        IReadOnlyList<DashboardUserGroupItemResponse> TicketsBySupervisor,
        DashboardAppliedFiltersResponse AppliedFilters);
    private sealed record DashboardGroupItemResponse(string Label, string Key, int Count, string? Status, string? Priority, string? SlaState, bool? IsEscalated, DateTime? DateFrom, DateTime? DateTo);
    private sealed record DashboardEntityGroupItemResponse(string Label, Guid EntityId, int Count, Guid? AccountId, Guid? CampaignId, DateTime? DateFrom, DateTime? DateTo);
    private sealed record DashboardUserGroupItemResponse(string Label, Guid? UserId, int Count, Guid? AssignedAgentUserId, Guid? SupervisorUserId, DateTime? DateFrom, DateTime? DateTo);
    private sealed record DashboardAppliedFiltersResponse(Guid? RegionId, Guid? CountryId, Guid? AccountId, Guid? CampaignId, Guid? SupervisorUserId, Guid? AgentUserId, string? Status, string? Priority, string? SlaState, DateTime? DateFrom, DateTime? DateTo);
}
