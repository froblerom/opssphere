using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using OpsSphere.Domain.Entities;
using OpsSphere.Domain.Enums;
using OpsSphere.Infrastructure.Persistence;
using OpsSphere.Infrastructure.Persistence.SeedData;
using OpsSphere.IntegrationTests.TestInfrastructure;

namespace OpsSphere.IntegrationTests.DashboardManagement;

public sealed class DashboardApiTests
{
    private const int SeededNovaBankTicketCount = 6;
    private const string AdminEmail = "admin@opssphere.local";
    private const string ManagerEmail = "manager.latam@opssphere.local";
    private const string SupervisorEmail = "supervisor.novabank@opssphere.local";
    private const string AgentEmail = "agent.novabank@opssphere.local";
    private const string ViewerEmail = "viewer.latam@opssphere.local";

    private static readonly JsonSerializerOptions JsonOptions = new(JsonSerializerDefaults.Web);

    [Fact]
    public async Task GetOperationalDashboard_WithoutAuth_Returns401()
    {
        await using var factory = await OpsSphereSqliteFactory.CreateAsync();

        var response = await factory.CreateClient().GetAsync("/api/dashboard/operational");

        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    [Fact]
    public async Task GetOperationalDashboard_WithAgent_Returns200()
    {
        await using var factory = await OpsSphereSqliteFactory.CreateAsync();
        var agent = await factory.CreateAuthenticatedClientAsync(AgentEmail);

        var response = await agent.GetAsync("/api/dashboard/operational");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
    }

    [Fact]
    public async Task GetOperationalDashboard_WithViewer_Returns200()
    {
        await using var factory = await OpsSphereSqliteFactory.CreateAsync();
        var viewer = await factory.CreateAuthenticatedClientAsync(ViewerEmail);

        var response = await viewer.GetAsync("/api/dashboard/operational");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
    }

    [Fact]
    public async Task GetOperationalDashboard_Manager_SeesOnlyRegionalTickets()
    {
        await using var factory = await OpsSphereSqliteFactory.CreateAsync();
        await AddTicketDirectlyAsync(factory, SeedIds.Accounts.NovaBank, SeedIds.Campaigns.NovaBankCreditCard, SeedIds.Customers.NovaBankCustomer1);
        await AddTicketDirectlyAsync(factory, SeedIds.Accounts.Streamly, SeedIds.Campaigns.StreamlyCreator, SeedIds.Customers.StreamlyCustomer1);
        var manager = await factory.CreateAuthenticatedClientAsync(ManagerEmail);

        var dashboard = await OpsSphereSqliteFactory.ReadDataAsync<OperationalDashboardResponse>(
            await manager.GetAsync("/api/dashboard/operational"));

        Assert.Equal(SeededNovaBankTicketCount + 1, dashboard.TotalTicketCount);
        Assert.Single(dashboard.TicketsByAccount);
        Assert.Equal(SeedIds.Accounts.NovaBank, dashboard.TicketsByAccount[0].EntityId);
    }

    [Fact]
    public async Task GetOperationalDashboard_Supervisor_SeesOnlyAccountScope()
    {
        await using var factory = await OpsSphereSqliteFactory.CreateAsync();
        await AddTicketDirectlyAsync(factory, SeedIds.Accounts.NovaBank, SeedIds.Campaigns.NovaBankCreditCard, SeedIds.Customers.NovaBankCustomer1);
        await AddTicketDirectlyAsync(factory, SeedIds.Accounts.Streamly, SeedIds.Campaigns.StreamlyCreator, SeedIds.Customers.StreamlyCustomer1);
        var supervisor = await factory.CreateAuthenticatedClientAsync(SupervisorEmail);

        var dashboard = await OpsSphereSqliteFactory.ReadDataAsync<OperationalDashboardResponse>(
            await supervisor.GetAsync("/api/dashboard/operational"));

        Assert.Equal(SeededNovaBankTicketCount + 1, dashboard.TotalTicketCount);
        Assert.Equal(SeedIds.Accounts.NovaBank, dashboard.TicketsByAccount[0].EntityId);
    }

    [Fact]
    public async Task GetOperationalDashboard_Agent_SeesOnlyCampaignScope()
    {
        await using var factory = await OpsSphereSqliteFactory.CreateAsync();
        await AddTicketDirectlyAsync(factory, SeedIds.Accounts.NovaBank, SeedIds.Campaigns.NovaBankCreditCard, SeedIds.Customers.NovaBankCustomer1);
        await AddTicketDirectlyAsync(factory, SeedIds.Accounts.NovaBank, SeedIds.Campaigns.NovaBankFraud, SeedIds.Customers.NovaBankCustomer1);
        var agent = await factory.CreateAuthenticatedClientAsync(AgentEmail);

        var dashboard = await OpsSphereSqliteFactory.ReadDataAsync<OperationalDashboardResponse>(
            await agent.GetAsync("/api/dashboard/operational"));

        Assert.Equal(SeededNovaBankTicketCount + 1, dashboard.TotalTicketCount);
        Assert.Equal(SeedIds.Campaigns.NovaBankCreditCard, dashboard.TicketsByCampaign[0].EntityId);
    }

    [Fact]
    public async Task GetOperationalDashboard_WithOutOfScopeAccountFilter_ReturnsZeroCounts()
    {
        await using var factory = await OpsSphereSqliteFactory.CreateAsync();
        await AddTicketDirectlyAsync(factory, SeedIds.Accounts.NovaBank, SeedIds.Campaigns.NovaBankCreditCard, SeedIds.Customers.NovaBankCustomer1);
        await AddTicketDirectlyAsync(factory, SeedIds.Accounts.Streamly, SeedIds.Campaigns.StreamlyCreator, SeedIds.Customers.StreamlyCustomer1);
        var supervisor = await factory.CreateAuthenticatedClientAsync(SupervisorEmail);

        var dashboard = await OpsSphereSqliteFactory.ReadDataAsync<OperationalDashboardResponse>(
            await supervisor.GetAsync($"/api/dashboard/operational?accountId={SeedIds.Accounts.Streamly}"));

        Assert.Equal(0, dashboard.TotalTicketCount);
        Assert.Empty(dashboard.TicketsByAccount);
    }

    [Fact]
    public async Task GetOperationalDashboard_Aggregates_AreCorrect()
    {
        await using var factory = await OpsSphereSqliteFactory.CreateAsync();
        await AddTicketDirectlyAsync(
            factory,
            SeedIds.Accounts.NovaBank,
            SeedIds.Campaigns.NovaBankCreditCard,
            SeedIds.Customers.NovaBankCustomer1,
            status: TicketStatus.Open,
            priority: TicketPriority.High,
            assignedAgentUserId: SeedIds.Users.AgentNovabank,
            supervisorUserId: SeedIds.Users.SupervisorNovabank,
            isEscalated: true);
        await AddTicketDirectlyAsync(
            factory,
            SeedIds.Accounts.NovaBank,
            SeedIds.Campaigns.NovaBankCreditCard,
            SeedIds.Customers.NovaBankCustomer1,
            status: TicketStatus.Assigned,
            priority: TicketPriority.Normal,
            assignedAgentUserId: SeedIds.Users.AgentNovabank);
        var agent = await factory.CreateAuthenticatedClientAsync(AgentEmail);

        var dashboard = await OpsSphereSqliteFactory.ReadDataAsync<OperationalDashboardResponse>(
            await agent.GetAsync("/api/dashboard/operational"));

        Assert.Equal(SeededNovaBankTicketCount + 2, dashboard.TotalTicketCount);
        Assert.Equal(2, dashboard.OpenTicketCount);
        Assert.Equal(7, dashboard.AssignedTicketCount);
        Assert.Equal(2, dashboard.EscalatedTicketCount);
        Assert.Contains(dashboard.TicketsByStatus, item => item.Key == "Open" && item.Count == 2);
        Assert.Contains(dashboard.TicketsByStatus, item => item.Key == "Assigned" && item.Count == 2);
        Assert.Contains(dashboard.TicketsByPriority, item => item.Key == "High" && item.Count == 3);
        Assert.Contains(dashboard.TicketsByAssignedAgent, item => item.UserId == SeedIds.Users.AgentNovabank && item.Count == 7);
        Assert.Contains(dashboard.TicketsBySupervisor, item => item.UserId == SeedIds.Users.SupervisorNovabank && item.Count == 6);
    }

    [Fact]
    public async Task GetOperationalDashboard_SlaAggregates_UseRequestTimeEvaluation()
    {
        await using var factory = await OpsSphereSqliteFactory.CreateAsync();
        await AddTicketDirectlyAsync(
            factory,
            SeedIds.Accounts.NovaBank,
            SeedIds.Campaigns.NovaBankCreditCard,
            SeedIds.Customers.NovaBankCustomer1,
            slaStartedAt: DateTime.UtcNow.AddHours(-6),
            slaDueAt: DateTime.UtcNow.AddHours(-1),
            storedSlaState: SlaState.WithinSla);
        var agent = await factory.CreateAuthenticatedClientAsync(AgentEmail);

        var dashboard = await OpsSphereSqliteFactory.ReadDataAsync<OperationalDashboardResponse>(
            await agent.GetAsync("/api/dashboard/operational"));

        Assert.Equal(3, dashboard.BreachedTicketCount);
        Assert.Contains(dashboard.TicketsBySlaState, item => item.Key == "Breached" && item.Count == 3);
    }

    [Fact]
    public async Task GetOperationalDashboard_WithAccountFilter_IntersectsScope()
    {
        await using var factory = await OpsSphereSqliteFactory.CreateAsync();
        await AddTicketDirectlyAsync(factory, SeedIds.Accounts.NovaBank, SeedIds.Campaigns.NovaBankCreditCard, SeedIds.Customers.NovaBankCustomer1);
        await AddTicketDirectlyAsync(factory, SeedIds.Accounts.NovaBank, SeedIds.Campaigns.NovaBankFraud, SeedIds.Customers.NovaBankCustomer1);
        var supervisor = await factory.CreateAuthenticatedClientAsync(SupervisorEmail);

        var dashboard = await OpsSphereSqliteFactory.ReadDataAsync<OperationalDashboardResponse>(
            await supervisor.GetAsync($"/api/dashboard/operational?accountId={SeedIds.Accounts.NovaBank}"));

        Assert.Equal(SeededNovaBankTicketCount + 2, dashboard.TotalTicketCount);
        Assert.All(dashboard.TicketsByAccount, item => Assert.Equal(SeedIds.Accounts.NovaBank, item.EntityId));
    }

    [Fact]
    public async Task GetOperationalDashboard_WithInvalidDateRange_Returns400()
    {
        await using var factory = await OpsSphereSqliteFactory.CreateAsync();
        var admin = await factory.CreateAuthenticatedClientAsync(AdminEmail);

        var response = await admin.GetAsync("/api/dashboard/operational?dateFrom=2026-05-18T00:00:00Z&dateTo=2026-05-17T00:00:00Z");

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    [Fact]
    public async Task GetOperationalDashboard_WithoutDashboardView_Returns403()
    {
        await using var factory = await OpsSphereSqliteFactory.CreateAsync();
        await RemoveDashboardPermissionFromAgentRoleAsync(factory);
        var agent = await factory.CreateAuthenticatedClientAsync(AgentEmail);

        var response = await agent.GetAsync("/api/dashboard/operational");

        Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);
    }

    private static async Task<Guid> AddTicketDirectlyAsync(
        OpsSphereSqliteFactory factory,
        Guid accountId,
        Guid campaignId,
        Guid customerId,
        TicketStatus status = TicketStatus.Open,
        TicketPriority priority = TicketPriority.Normal,
        Guid? assignedAgentUserId = null,
        Guid? supervisorUserId = null,
        bool isEscalated = false,
        SlaState storedSlaState = SlaState.WithinSla,
        DateTime? slaStartedAt = null,
        DateTime? slaDueAt = null)
    {
        using var scope = factory.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<OpsSphereDbContext>();
        var campaign = await db.Campaigns
            .AsNoTracking()
            .Where(c => c.Id == campaignId)
            .Select(c => new { c.CountryId, c.Country.RegionId })
            .SingleAsync();

        var ticketId = Guid.NewGuid();
        var now = DateTime.UtcNow;
        var startedAt = slaStartedAt ?? now;
        var dueAt = slaDueAt ?? now.AddHours(24);

        db.Tickets.Add(new Ticket
        {
            Id = ticketId,
            TicketNumber = $"OPS-20990103-{Random.Shared.Next(1, 999999):D6}",
            CustomerId = customerId,
            RegionId = campaign.RegionId,
            CountryId = campaign.CountryId,
            AccountId = accountId,
            CampaignId = campaignId,
            CreatedByUserId = SeedIds.Users.SupervisorNovabank,
            AssignedAgentUserId = assignedAgentUserId,
            SupervisorUserId = supervisorUserId,
            Category = "Support",
            Priority = priority,
            Status = status,
            Subject = "Dashboard test ticket",
            Description = "Dashboard integration test ticket.",
            SlaState = storedSlaState,
            SlaDueAt = dueAt,
            IsEscalated = isEscalated,
            IsDeleted = false,
            CreatedAt = now
        });
        db.TicketSlaStates.Add(new TicketSlaState
        {
            Id = Guid.NewGuid(),
            TicketId = ticketId,
            StartedAt = startedAt,
            DueAt = dueAt,
            AtRiskThresholdPercent = 80,
            State = storedSlaState.ToString()
        });
        await db.SaveChangesAsync();
        return ticketId;
    }

    private static async Task RemoveDashboardPermissionFromAgentRoleAsync(OpsSphereSqliteFactory factory)
    {
        using var scope = factory.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<OpsSphereDbContext>();
        var mapping = await db.RolePermissions
            .SingleAsync(rp => rp.RoleId == SeedIds.Roles.Agent && rp.PermissionId == SeedIds.Permissions.DashboardView);
        db.RolePermissions.Remove(mapping);
        await db.SaveChangesAsync();
    }

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

    private sealed record DashboardGroupItemResponse(
        string Label,
        string Key,
        int Count,
        string? Status,
        string? Priority,
        string? SlaState,
        bool? IsEscalated,
        DateTime? DateFrom,
        DateTime? DateTo);

    private sealed record DashboardEntityGroupItemResponse(
        string Label,
        Guid EntityId,
        int Count,
        Guid? AccountId,
        Guid? CampaignId,
        DateTime? DateFrom,
        DateTime? DateTo);

    private sealed record DashboardUserGroupItemResponse(
        string Label,
        Guid? UserId,
        int Count,
        Guid? AssignedAgentUserId,
        Guid? SupervisorUserId,
        DateTime? DateFrom,
        DateTime? DateTo);

    private sealed record DashboardAppliedFiltersResponse(
        Guid? RegionId,
        Guid? CountryId,
        Guid? AccountId,
        Guid? CampaignId,
        Guid? SupervisorUserId,
        Guid? AgentUserId,
        string? Status,
        string? Priority,
        string? SlaState,
        DateTime? DateFrom,
        DateTime? DateTo);
}