using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using OpsSphere.Domain.Authorization;
using OpsSphere.Domain.Entities;
using OpsSphere.Domain.Enums;
using OpsSphere.Infrastructure.Persistence;
using OpsSphere.Infrastructure.Persistence.SeedData;
using OpsSphere.IntegrationTests.TestInfrastructure;

namespace OpsSphere.IntegrationTests.TicketManagement;

public sealed class TicketAssignmentApiTests
{
    private const string AdminEmail = "admin@opssphere.local";
    private const string AgentEmail = "agent.novabank@opssphere.local";
    private const string SupervisorEmail = "supervisor.novabank@opssphere.local";
    private const string ViewerEmail = "viewer.latam@opssphere.local";

    private static readonly JsonSerializerOptions JsonOptions = new(JsonSerializerDefaults.Web);

    [Fact]
    public async Task Supervisor_CanAssignTicket_WithinScope_Returns200()
    {
        await using var factory = await OpsSphereSqliteFactory.CreateAsync();
        var supervisor = await factory.CreateAuthenticatedClientAsync(SupervisorEmail);
        var ticket = await CreateNovaBankTicketAsync(supervisor, "Assignment happy path");

        var response = await AssignTicketAsync(supervisor, ticket.Id, SeedIds.Users.AgentNovabank);

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var result = await OpsSphereSqliteFactory.ReadDataAsync<AssignTicketResponse>(response);
        Assert.Equal(ticket.Id, result.TicketId);
        Assert.Equal(ticket.TicketNumber, result.TicketNumber);
        Assert.Equal(SeedIds.Users.AgentNovabank, result.AssignedAgentUserId);
        Assert.Null(result.PreviousAgentUserId);
        Assert.Equal("Assigned", result.Status);
        Assert.Equal("Ticket assigned successfully.", result.Message);
    }

    [Fact]
    public async Task Supervisor_CanReassignTicket_Returns200()
    {
        await using var factory = await OpsSphereSqliteFactory.CreateAsync();
        var supervisor = await factory.CreateAuthenticatedClientAsync(SupervisorEmail);
        var ticket = await CreateNovaBankTicketAsync(supervisor, "Reassignment happy path");
        await AssignTicketAsync(supervisor, ticket.Id, SeedIds.Users.AgentNovabank);
        var secondAgentId = await AddAgentAsync(factory, "Reassign Target", isActive: true, scopeType: ScopeTypes.Account, accountId: SeedIds.Accounts.NovaBank);

        var response = await AssignTicketAsync(supervisor, ticket.Id, secondAgentId, "Routing to backup agent");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var result = await OpsSphereSqliteFactory.ReadDataAsync<AssignTicketResponse>(response);
        Assert.Equal(secondAgentId, result.AssignedAgentUserId);
        Assert.Equal(SeedIds.Users.AgentNovabank, result.PreviousAgentUserId);
        Assert.Equal("Assigned", result.Status);
    }

    [Fact]
    public async Task Agent_CannotAssignTicket_Returns403()
    {
        await using var factory = await OpsSphereSqliteFactory.CreateAsync();
        var supervisor = await factory.CreateAuthenticatedClientAsync(SupervisorEmail);
        var agent = await factory.CreateAuthenticatedClientAsync(AgentEmail);
        var ticket = await CreateNovaBankTicketAsync(supervisor, "Agent forbidden assignment");

        var response = await AssignTicketAsync(agent, ticket.Id, SeedIds.Users.AgentNovabank);

        Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);
    }

    [Fact]
    public async Task Viewer_CannotAssignTicket_Returns403()
    {
        await using var factory = await OpsSphereSqliteFactory.CreateAsync();
        var supervisor = await factory.CreateAuthenticatedClientAsync(SupervisorEmail);
        var viewer = await factory.CreateAuthenticatedClientAsync(ViewerEmail);
        var ticket = await CreateNovaBankTicketAsync(supervisor, "Viewer forbidden assignment");

        var response = await AssignTicketAsync(viewer, ticket.Id, SeedIds.Users.AgentNovabank);

        Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);
    }

    [Fact]
    public async Task Admin_CannotAssignTicket_Returns403()
    {
        await using var factory = await OpsSphereSqliteFactory.CreateAsync();
        var supervisor = await factory.CreateAuthenticatedClientAsync(SupervisorEmail);
        var admin = await factory.CreateAuthenticatedClientAsync(AdminEmail);
        var ticket = await CreateNovaBankTicketAsync(supervisor, "Admin forbidden assignment");

        var response = await AssignTicketAsync(admin, ticket.Id, SeedIds.Users.AgentNovabank);

        Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);
    }

    [Fact]
    public async Task Supervisor_CannotAssign_OutOfScopeTicket_Returns404()
    {
        await using var factory = await OpsSphereSqliteFactory.CreateAsync();
        var supervisor = await factory.CreateAuthenticatedClientAsync(SupervisorEmail);
        var outOfScopeTicketId = await AddTicketDirectlyAsync(
            factory,
            SeedIds.Accounts.Streamly,
            SeedIds.Campaigns.StreamlyCreator,
            SeedIds.Customers.StreamlyCustomer1,
            TicketStatus.Open,
            "Out-of-scope assignment ticket");

        var response = await AssignTicketAsync(supervisor, outOfScopeTicketId, SeedIds.Users.AgentNovabank);

        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }

    [Fact]
    public async Task Supervisor_CannotAssign_ClosedTicket_Returns400()
    {
        await using var factory = await OpsSphereSqliteFactory.CreateAsync();
        var supervisor = await factory.CreateAuthenticatedClientAsync(SupervisorEmail);
        var ticket = await CreateNovaBankTicketAsync(supervisor, "Closed assignment rejection");
        await SetTicketStatusAsync(factory, ticket.Id, TicketStatus.Closed);

        var response = await AssignTicketAsync(supervisor, ticket.Id, SeedIds.Users.AgentNovabank);

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        var error = await OpsSphereSqliteFactory.ReadErrorAsync(response);
        Assert.Equal("business_rule_violation", error.Error.Code);
        Assert.Contains("closed", error.Error.Message, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public async Task Supervisor_CannotAssign_InactiveAgent_Returns400()
    {
        await using var factory = await OpsSphereSqliteFactory.CreateAsync();
        var supervisor = await factory.CreateAuthenticatedClientAsync(SupervisorEmail);
        var ticket = await CreateNovaBankTicketAsync(supervisor, "Inactive agent rejection");
        var inactiveAgentId = await AddAgentAsync(factory, "Inactive Test Agent", isActive: false, scopeType: ScopeTypes.Campaign, campaignId: SeedIds.Campaigns.NovaBankCreditCard);

        var response = await AssignTicketAsync(supervisor, ticket.Id, inactiveAgentId);

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        var error = await OpsSphereSqliteFactory.ReadErrorAsync(response);
        Assert.Equal("validation_error", error.Error.Code);
        Assert.Contains(error.Error.Details, d => d.Field == "targetAgentUserId");
    }

    [Fact]
    public async Task Supervisor_CannotAssign_OutOfScopeAgent_Returns400()
    {
        await using var factory = await OpsSphereSqliteFactory.CreateAsync();
        var supervisor = await factory.CreateAuthenticatedClientAsync(SupervisorEmail);
        var ticket = await CreateNovaBankTicketAsync(supervisor, "Out-of-scope agent rejection");
        var outOfScopeAgentId = await AddAgentAsync(factory, "Streamly Test Agent", isActive: true, scopeType: ScopeTypes.Campaign, campaignId: SeedIds.Campaigns.StreamlyCreator);

        var response = await AssignTicketAsync(supervisor, ticket.Id, outOfScopeAgentId);

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        var error = await OpsSphereSqliteFactory.ReadErrorAsync(response);
        Assert.Equal("validation_error", error.Error.Code);
        Assert.Contains(error.Error.Details, d => d.Field == "targetAgentUserId");
    }

    [Fact]
    public async Task Supervisor_CannotAssign_SameAgent_Returns400()
    {
        await using var factory = await OpsSphereSqliteFactory.CreateAsync();
        var supervisor = await factory.CreateAuthenticatedClientAsync(SupervisorEmail);
        var ticket = await CreateNovaBankTicketAsync(supervisor, "Same-agent assignment rejection");
        await AssignTicketAsync(supervisor, ticket.Id, SeedIds.Users.AgentNovabank);

        var response = await AssignTicketAsync(supervisor, ticket.Id, SeedIds.Users.AgentNovabank);

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        var error = await OpsSphereSqliteFactory.ReadErrorAsync(response);
        Assert.Equal("business_rule_violation", error.Error.Code);
        Assert.Contains("already assigned", error.Error.Message, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public async Task Assign_UpdatesStatus_OpenToAssigned()
    {
        await using var factory = await OpsSphereSqliteFactory.CreateAsync();
        var supervisor = await factory.CreateAuthenticatedClientAsync(SupervisorEmail);
        var ticket = await CreateNovaBankTicketAsync(supervisor, "Open to assigned status");

        await AssignTicketAsync(supervisor, ticket.Id, SeedIds.Users.AgentNovabank);

        using var scope = factory.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<OpsSphereDbContext>();
        var updated = await db.Tickets.AsNoTracking().SingleAsync(t => t.Id == ticket.Id);
        Assert.Equal(TicketStatus.Assigned, updated.Status);
        Assert.Equal(SeedIds.Users.AgentNovabank, updated.AssignedAgentUserId);
    }

    [Fact]
    public async Task Assign_DoesNotChangeStatus_IfAlreadyAssigned()
    {
        await using var factory = await OpsSphereSqliteFactory.CreateAsync();
        var supervisor = await factory.CreateAuthenticatedClientAsync(SupervisorEmail);
        var ticket = await CreateNovaBankTicketAsync(supervisor, "Assigned status remains assigned");
        var secondAgentId = await AddAgentAsync(factory, "Second Assigned Agent", isActive: true, scopeType: ScopeTypes.Account, accountId: SeedIds.Accounts.NovaBank);
        await AssignTicketAsync(supervisor, ticket.Id, SeedIds.Users.AgentNovabank);

        var response = await AssignTicketAsync(supervisor, ticket.Id, secondAgentId);

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        using var scope = factory.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<OpsSphereDbContext>();
        var updated = await db.Tickets.AsNoTracking().SingleAsync(t => t.Id == ticket.Id);
        Assert.Equal(TicketStatus.Assigned, updated.Status);
    }

    [Fact]
    public async Task Assign_CreatesAssignmentHistory()
    {
        await using var factory = await OpsSphereSqliteFactory.CreateAsync();
        var supervisor = await factory.CreateAuthenticatedClientAsync(SupervisorEmail);
        var ticket = await CreateNovaBankTicketAsync(supervisor, "Assignment history persistence");

        await AssignTicketAsync(supervisor, ticket.Id, SeedIds.Users.AgentNovabank, "  Trimmed reason  ");

        using var scope = factory.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<OpsSphereDbContext>();
        var assignment = await db.TicketAssignments.AsNoTracking().SingleAsync(a => a.TicketId == ticket.Id);
        Assert.Null(assignment.PreviousAgentUserId);
        Assert.Equal(SeedIds.Users.AgentNovabank, assignment.NewAgentUserId);
        Assert.Equal(SeedIds.Users.SupervisorNovabank, assignment.AssignedByUserId);
        Assert.Equal("Trimmed reason", assignment.AssignmentReason);
    }

    [Fact]
    public async Task Assign_CreatesStatusHistory_WhenOpenToAssigned()
    {
        await using var factory = await OpsSphereSqliteFactory.CreateAsync();
        var supervisor = await factory.CreateAuthenticatedClientAsync(SupervisorEmail);
        var ticket = await CreateNovaBankTicketAsync(supervisor, "Assignment status history");

        await AssignTicketAsync(supervisor, ticket.Id, SeedIds.Users.AgentNovabank);

        using var scope = factory.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<OpsSphereDbContext>();
        var history = await db.TicketStatusHistory
            .AsNoTracking()
            .Where(h => h.TicketId == ticket.Id && h.PreviousStatus == "Open" && h.NewStatus == "Assigned")
            .SingleOrDefaultAsync();

        Assert.NotNull(history);
        Assert.Equal(SeedIds.Users.SupervisorNovabank, history.ChangedByUserId);
        Assert.Equal("Ticket assigned", history.ChangeReason);
    }

    [Fact]
    public async Task Assign_DoesNotCreateStatusHistory_WhenStatusUnchanged()
    {
        await using var factory = await OpsSphereSqliteFactory.CreateAsync();
        var supervisor = await factory.CreateAuthenticatedClientAsync(SupervisorEmail);
        var ticket = await CreateNovaBankTicketAsync(supervisor, "No status history on reassignment");
        var secondAgentId = await AddAgentAsync(factory, "No Status History Agent", isActive: true, scopeType: ScopeTypes.Account, accountId: SeedIds.Accounts.NovaBank);
        await AssignTicketAsync(supervisor, ticket.Id, SeedIds.Users.AgentNovabank);
        var before = await CountStatusHistoryAsync(factory, ticket.Id);

        await AssignTicketAsync(supervisor, ticket.Id, secondAgentId);

        var after = await CountStatusHistoryAsync(factory, ticket.Id);
        Assert.Equal(before, after);
    }

    [Fact]
    public async Task Assign_CreatesAuditLog_WithTicketAssignedAction()
    {
        await using var factory = await OpsSphereSqliteFactory.CreateAsync();
        var supervisor = await factory.CreateAuthenticatedClientAsync(SupervisorEmail);
        var ticket = await CreateNovaBankTicketAsync(supervisor, "Assignment audit action");

        await AssignTicketAsync(supervisor, ticket.Id, SeedIds.Users.AgentNovabank);

        using var scope = factory.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<OpsSphereDbContext>();
        var audit = await db.AuditLogs
            .AsNoTracking()
            .SingleOrDefaultAsync(log => log.Action == "TicketAssigned" && log.EntityType == "Ticket" && log.EntityId == ticket.Id);

        Assert.NotNull(audit);
        Assert.False(string.IsNullOrWhiteSpace(audit.CorrelationId));
    }

    [Fact]
    public async Task AuditLog_DoesNotContainPII_OnAssignment()
    {
        await using var factory = await OpsSphereSqliteFactory.CreateAsync();
        var supervisor = await factory.CreateAuthenticatedClientAsync(SupervisorEmail);
        const string sensitiveSubject = "AssignmentSensitiveSubject-ABC123";
        const string sensitiveDescription = "AssignmentSensitiveDescription-XYZ987";
        var ticket = await CreateTicketAsync(
            supervisor,
            SeedIds.Customers.NovaBankCustomer1,
            SeedIds.Accounts.NovaBank,
            SeedIds.Campaigns.NovaBankCreditCard,
            "Billing",
            "Normal",
            sensitiveSubject,
            sensitiveDescription);

        await AssignTicketAsync(supervisor, ticket.Id, SeedIds.Users.AgentNovabank, "Free text reason should stay out of audit");

        using var scope = factory.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<OpsSphereDbContext>();
        var audit = await db.AuditLogs
            .AsNoTracking()
            .SingleAsync(log => log.Action == "TicketAssigned" && log.EntityId == ticket.Id);
        var combined = $"{audit.PreviousValue}{audit.NewValue}";

        Assert.DoesNotContain(sensitiveSubject, combined, StringComparison.OrdinalIgnoreCase);
        Assert.DoesNotContain(sensitiveDescription, combined, StringComparison.OrdinalIgnoreCase);
        Assert.DoesNotContain("Carlos", combined, StringComparison.OrdinalIgnoreCase);
        Assert.DoesNotContain("Mendez", combined, StringComparison.OrdinalIgnoreCase);
        Assert.DoesNotContain("Diego", combined, StringComparison.OrdinalIgnoreCase);
        Assert.DoesNotContain("Santos", combined, StringComparison.OrdinalIgnoreCase);
        Assert.DoesNotContain(AgentEmail, combined, StringComparison.OrdinalIgnoreCase);
        Assert.DoesNotContain("Free text reason", combined, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public async Task GetEligibleAgents_ReturnsActiveAgentsInScope_Returns200()
    {
        await using var factory = await OpsSphereSqliteFactory.CreateAsync();
        var supervisor = await factory.CreateAuthenticatedClientAsync(SupervisorEmail);
        var ticket = await CreateNovaBankTicketAsync(supervisor, "Eligible agents lookup");

        var response = await supervisor.GetAsync($"/api/tickets/{ticket.Id}/eligible-agents");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var agents = await OpsSphereSqliteFactory.ReadDataAsync<IReadOnlyList<EligibleAgentDto>>(response);
        Assert.Contains(agents, a => a.UserId == SeedIds.Users.AgentNovabank && a.DisplayName == "Diego Santos");
    }

    [Fact]
    public async Task GetEligibleAgents_Returns404_ForOutOfScopeTicket()
    {
        await using var factory = await OpsSphereSqliteFactory.CreateAsync();
        var supervisor = await factory.CreateAuthenticatedClientAsync(SupervisorEmail);
        var outOfScopeTicketId = await AddTicketDirectlyAsync(
            factory,
            SeedIds.Accounts.Streamly,
            SeedIds.Campaigns.StreamlyCreator,
            SeedIds.Customers.StreamlyCustomer1,
            TicketStatus.Open,
            "Out-of-scope eligible lookup ticket");

        var response = await supervisor.GetAsync($"/api/tickets/{outOfScopeTicketId}/eligible-agents");

        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }

    [Fact]
    public async Task GetEligibleAgents_DoesNotReturnInactiveOrOutOfScopeAgents()
    {
        await using var factory = await OpsSphereSqliteFactory.CreateAsync();
        var supervisor = await factory.CreateAuthenticatedClientAsync(SupervisorEmail);
        var ticket = await CreateNovaBankTicketAsync(supervisor, "Eligible agents filtering");
        var inactiveAgentId = await AddAgentAsync(factory, "Inactive Lookup Agent", isActive: false, scopeType: ScopeTypes.Campaign, campaignId: SeedIds.Campaigns.NovaBankCreditCard);
        var outOfScopeAgentId = await AddAgentAsync(factory, "Out Of Scope Lookup Agent", isActive: true, scopeType: ScopeTypes.Campaign, campaignId: SeedIds.Campaigns.StreamlyCreator);
        var regionOnlyAgentId = await AddAgentAsync(factory, "Region Only Lookup Agent", isActive: true, scopeType: ScopeTypes.Region, regionId: SeedIds.Regions.Latam);

        var response = await supervisor.GetAsync($"/api/tickets/{ticket.Id}/eligible-agents");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var agents = await OpsSphereSqliteFactory.ReadDataAsync<IReadOnlyList<EligibleAgentDto>>(response);
        Assert.Contains(agents, a => a.UserId == SeedIds.Users.AgentNovabank);
        Assert.DoesNotContain(agents, a => a.UserId == inactiveAgentId);
        Assert.DoesNotContain(agents, a => a.UserId == outOfScopeAgentId);
        Assert.DoesNotContain(agents, a => a.UserId == regionOnlyAgentId);
    }

    private static Task<HttpResponseMessage> AssignTicketAsync(
        HttpClient client,
        Guid ticketId,
        Guid targetAgentUserId,
        string? reassignmentReason = null)
    {
        return client.PostAsJsonAsync($"/api/tickets/{ticketId}/assign", new
        {
            targetAgentUserId,
            reassignmentReason
        });
    }

    private static async Task<CreateTicketResult> CreateNovaBankTicketAsync(HttpClient client, string subject) =>
        await CreateTicketAsync(
            client,
            SeedIds.Customers.NovaBankCustomer1,
            SeedIds.Accounts.NovaBank,
            SeedIds.Campaigns.NovaBankCreditCard,
            "Support",
            "Normal",
            subject,
            $"Description for {subject}");

    private static async Task<CreateTicketResult> CreateTicketAsync(
        HttpClient client, Guid customerId, Guid accountId, Guid campaignId,
        string category, string priority, string subject, string description)
    {
        var response = await client.PostAsJsonAsync("/api/tickets", new
        {
            customerId,
            accountId,
            campaignId,
            category,
            priority,
            subject,
            description
        });
        Assert.Equal(HttpStatusCode.Created, response.StatusCode);
        return await OpsSphereSqliteFactory.ReadDataAsync<CreateTicketResult>(response);
    }

    private static async Task<Guid> AddAgentAsync(
        OpsSphereSqliteFactory factory,
        string displayName,
        bool isActive,
        string scopeType,
        Guid? accountId = null,
        Guid? campaignId = null,
        Guid? regionId = null,
        Guid? countryId = null)
    {
        using var scope = factory.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<OpsSphereDbContext>();
        var userId = Guid.NewGuid();
        db.Users.Add(new User
        {
            Id = userId,
            Email = $"{userId:N}@assignment-tests.local",
            PasswordHash = "not-used-in-tests",
            FirstName = displayName.Split(' ', StringSplitOptions.RemoveEmptyEntries).FirstOrDefault() ?? "Test",
            LastName = displayName.Split(' ', StringSplitOptions.RemoveEmptyEntries).Skip(1).FirstOrDefault() ?? "Agent",
            DisplayName = displayName,
            IsActive = isActive,
            DeactivatedAt = isActive ? null : DateTime.UtcNow
        });
        db.UserRoles.Add(new UserRole
        {
            UserId = userId,
            RoleId = SeedIds.Roles.Agent
        });
        db.UserScopes.Add(new UserScope
        {
            Id = Guid.NewGuid(),
            UserId = userId,
            ScopeType = scopeType,
            RegionId = regionId,
            CountryId = countryId,
            AccountId = accountId,
            CampaignId = campaignId,
            IsActive = true,
            CreatedByUserId = SeedIds.Users.SupervisorNovabank
        });
        await db.SaveChangesAsync();
        return userId;
    }

    private static async Task<Guid> AddTicketDirectlyAsync(
        OpsSphereSqliteFactory factory,
        Guid accountId,
        Guid campaignId,
        Guid customerId,
        TicketStatus status,
        string subject)
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
        db.Tickets.Add(new Ticket
        {
            Id = ticketId,
            TicketNumber = $"OPS-20990101-{Random.Shared.Next(1, 999999):D6}",
            CustomerId = customerId,
            RegionId = campaign.RegionId,
            CountryId = campaign.CountryId,
            AccountId = accountId,
            CampaignId = campaignId,
            CreatedByUserId = SeedIds.Users.SupervisorNovabank,
            Category = "Support",
            Priority = TicketPriority.Normal,
            Status = status,
            Subject = subject,
            Description = $"Description for {subject}",
            SlaState = SlaState.WithinSla,
            SlaDueAt = now.AddHours(24),
            IsEscalated = false,
            IsDeleted = false,
            CreatedAt = now
        });
        db.TicketStatusHistory.Add(new TicketStatusHistory
        {
            Id = Guid.NewGuid(),
            TicketId = ticketId,
            PreviousStatus = null,
            NewStatus = status.ToString(),
            ChangedByUserId = SeedIds.Users.SupervisorNovabank,
            ChangeReason = "Test arrange",
            CreatedAt = now
        });
        db.TicketSlaStates.Add(new TicketSlaState
        {
            Id = Guid.NewGuid(),
            TicketId = ticketId,
            SlaPolicyId = null,
            StartedAt = now,
            DueAt = now.AddHours(24),
            State = SlaState.WithinSla.ToString()
        });
        await db.SaveChangesAsync();
        return ticketId;
    }

    private static async Task SetTicketStatusAsync(OpsSphereSqliteFactory factory, Guid ticketId, TicketStatus status)
    {
        using var scope = factory.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<OpsSphereDbContext>();
        var ticket = await db.Tickets.SingleAsync(t => t.Id == ticketId);
        ticket.Status = status;
        await db.SaveChangesAsync();
    }

    private static async Task<int> CountStatusHistoryAsync(OpsSphereSqliteFactory factory, Guid ticketId)
    {
        using var scope = factory.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<OpsSphereDbContext>();
        return await db.TicketStatusHistory.AsNoTracking().CountAsync(h => h.TicketId == ticketId);
    }

    private sealed record CreateTicketResult(
        Guid Id,
        string TicketNumber,
        string Status,
        string Priority,
        string SlaState,
        DateTime? SlaDueAt);

    private sealed record AssignTicketResponse(
        Guid TicketId,
        string TicketNumber,
        Guid AssignedAgentUserId,
        Guid? PreviousAgentUserId,
        string Status,
        string Message);

    private sealed record EligibleAgentDto(
        Guid UserId,
        string DisplayName,
        string? ScopeType,
        string? ScopeReference);
}