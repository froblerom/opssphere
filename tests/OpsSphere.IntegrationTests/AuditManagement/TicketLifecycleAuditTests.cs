using System.Net;
using System.Net.Http.Json;
using OpsSphere.Infrastructure.Persistence.SeedData;
using OpsSphere.IntegrationTests.TestInfrastructure;

namespace OpsSphere.IntegrationTests.AuditManagement;

public sealed class TicketLifecycleAuditTests
{
    private const string AgentEmail = "agent.novabank@opssphere.local";
    private const string SupervisorEmail = "supervisor.novabank@opssphere.local";

    [Fact]
    public async Task StatusUpdate_ProducesAuditLog_WithAction_TicketStatusChanged()
    {
        await using var factory = await OpsSphereSqliteFactory.CreateAsync();
        var supervisor = await factory.CreateAuthenticatedClientAsync(SupervisorEmail);

        var created = await CreateAndAssignTicketAsync(supervisor);
        var statusResponse = await supervisor.PutAsJsonAsync($"/api/tickets/{created.Id}/status", new
        {
            status = "InProgress",
            changeReason = "Audit test status update"
        });
        statusResponse.EnsureSuccessStatusCode();

        await factory.AssertAuditAsync("TicketStatusChanged", "Ticket", created.Id);
    }

    [Fact]
    public async Task PriorityUpdate_ProducesAuditLog_WithAction_TicketPriorityChanged()
    {
        await using var factory = await OpsSphereSqliteFactory.CreateAsync();
        var agent = await factory.CreateAuthenticatedClientAsync(AgentEmail);

        var created = await CreateTicketAsync(agent);
        var priorityResponse = await agent.PutAsJsonAsync($"/api/tickets/{created.Id}/priority", new
        {
            priority = "High",
            changeReason = "Audit test priority update"
        });
        priorityResponse.EnsureSuccessStatusCode();

        await factory.AssertAuditAsync("TicketPriorityChanged", "Ticket", created.Id);
    }

    [Fact]
    public async Task Escalation_ProducesAuditLog_WithAction_TicketEscalated()
    {
        await using var factory = await OpsSphereSqliteFactory.CreateAsync();
        var supervisor = await factory.CreateAuthenticatedClientAsync(SupervisorEmail);

        var created = await CreateAndAssignTicketAsync(supervisor);
        var escalateResponse = await supervisor.PostAsJsonAsync($"/api/tickets/{created.Id}/escalate", new
        {
            escalationReason = "Audit test escalation"
        });
        escalateResponse.EnsureSuccessStatusCode();

        await factory.AssertAuditAsync("TicketEscalated", "Ticket", created.Id);
    }

    [Fact]
    public async Task Resolve_ProducesAuditLog_WithAction_TicketResolved()
    {
        await using var factory = await OpsSphereSqliteFactory.CreateAsync();
        var supervisor = await factory.CreateAuthenticatedClientAsync(SupervisorEmail);

        var created = await CreateAndAssignTicketAsync(supervisor);
        var resolveResponse = await supervisor.PostAsJsonAsync($"/api/tickets/{created.Id}/resolve", new
        {
            resolutionSummary = "Audit test resolution",
            resolutionCode = "RESOLVED"
        });
        resolveResponse.EnsureSuccessStatusCode();

        await factory.AssertAuditAsync("TicketResolved", "Ticket", created.Id);
    }

    [Fact]
    public async Task Close_ProducesAuditLog_WithAction_TicketClosed()
    {
        await using var factory = await OpsSphereSqliteFactory.CreateAsync();
        var supervisor = await factory.CreateAuthenticatedClientAsync(SupervisorEmail);

        var created = await CreateAndAssignTicketAsync(supervisor);
        var resolveResponse = await supervisor.PostAsJsonAsync($"/api/tickets/{created.Id}/resolve", new
        {
            resolutionSummary = "Resolved before close",
            resolutionCode = "RESOLVED"
        });
        resolveResponse.EnsureSuccessStatusCode();

        var closeResponse = await supervisor.PostAsync($"/api/tickets/{created.Id}/close", null);
        closeResponse.EnsureSuccessStatusCode();

        await factory.AssertAuditAsync("TicketClosed", "Ticket", created.Id);
    }

    [Fact]
    public async Task AddComment_ProducesAuditLog_WithAction_InternalCommentAdded()
    {
        await using var factory = await OpsSphereSqliteFactory.CreateAsync();
        var supervisor = await factory.CreateAuthenticatedClientAsync(SupervisorEmail);

        var created = await CreateTicketAsync(supervisor);
        var commentResponse = await supervisor.PostAsJsonAsync($"/api/tickets/{created.Id}/comments", new
        {
            body = "Audit test internal comment"
        });
        commentResponse.EnsureSuccessStatusCode();

        await factory.AssertAuditAsync("InternalCommentAdded", "Ticket", created.Id);
    }

    [Fact]
    public async Task TicketAuditLogs_QueryableViaEntityEndpoint_ReturnAllLifecycleActions()
    {
        await using var factory = await OpsSphereSqliteFactory.CreateAsync();
        var supervisor = await factory.CreateAuthenticatedClientAsync(SupervisorEmail);

        var created = await CreateAndAssignTicketAsync(supervisor);
        var priorityResponse = await supervisor.PutAsJsonAsync($"/api/tickets/{created.Id}/priority", new
        {
            priority = "Critical",
            changeReason = "Lifecycle query test"
        });
        priorityResponse.EnsureSuccessStatusCode();

        var entityAuditResponse = await supervisor.GetAsync($"/api/audit-logs/entity/Ticket/{created.Id}");
        Assert.Equal(HttpStatusCode.OK, entityAuditResponse.StatusCode);
        var body = await OpsSphereSqliteFactory.ReadResponseAsync<PagedAuditListResponse>(entityAuditResponse);

        Assert.Contains(body.Data, item => item.Action == "TicketCreated");
        Assert.Contains(body.Data, item => item.Action == "TicketAssigned");
        Assert.Contains(body.Data, item => item.Action == "TicketPriorityChanged");
    }

    [Fact]
    public async Task TicketAuditLog_ById_ReturnsDetail_WithoutSensitiveData()
    {
        await using var factory = await OpsSphereSqliteFactory.CreateAsync();
        var supervisor = await factory.CreateAuthenticatedClientAsync(SupervisorEmail);

        var created = await CreateTicketAsync(supervisor);
        var priorityResponse = await supervisor.PutAsJsonAsync($"/api/tickets/{created.Id}/priority", new
        {
            priority = "High",
            changeReason = "Sensitive data audit check"
        });
        priorityResponse.EnsureSuccessStatusCode();

        var auditListResponse = await supervisor.GetAsync($"/api/audit-logs?entityType=Ticket&entityId={created.Id}&action=TicketPriorityChanged");
        Assert.Equal(HttpStatusCode.OK, auditListResponse.StatusCode);
        var auditList = await OpsSphereSqliteFactory.ReadResponseAsync<PagedAuditListResponse>(auditListResponse);
        var auditItem = Assert.Single(auditList.Data);

        var detailResponse = await supervisor.GetAsync($"/api/audit-logs/{auditItem.Id}");
        Assert.Equal(HttpStatusCode.OK, detailResponse.StatusCode);
        var detail = await OpsSphereSqliteFactory.ReadDataAsync<AuditLogDetailResponse>(detailResponse);

        Assert.Equal("TicketPriorityChanged", detail.Action);
        Assert.Equal("Ticket", detail.EntityType);
        Assert.Equal(created.Id, detail.EntityId);
        Assert.DoesNotContain("password", detail.NewValue ?? string.Empty, StringComparison.OrdinalIgnoreCase);
        Assert.DoesNotContain("password", detail.PreviousValue ?? string.Empty, StringComparison.OrdinalIgnoreCase);
        Assert.DoesNotContain(OpsSphereSqliteFactory.JwtSigningKey, detail.NewValue ?? string.Empty, StringComparison.Ordinal);
    }

    private static async Task<CreateTicketResult> CreateTicketAsync(HttpClient client)
    {
        var response = await client.PostAsJsonAsync("/api/tickets", new
        {
            customerId = SeedIds.Customers.NovaBankCustomer1,
            accountId = SeedIds.Accounts.NovaBank,
            campaignId = SeedIds.Campaigns.NovaBankCreditCard,
            category = "Support",
            priority = "Normal",
            subject = "Lifecycle audit test ticket",
            description = "Testing ticket lifecycle audit log production"
        });
        Assert.Equal(HttpStatusCode.Created, response.StatusCode);
        return await OpsSphereSqliteFactory.ReadDataAsync<CreateTicketResult>(response);
    }

    private static async Task<CreateTicketResult> CreateAndAssignTicketAsync(HttpClient supervisorClient)
    {
        var created = await CreateTicketAsync(supervisorClient);
        var assignResponse = await supervisorClient.PostAsJsonAsync($"/api/tickets/{created.Id}/assign", new
        {
            targetAgentUserId = SeedIds.Users.AgentNovabank,
            reassignmentReason = (string?)null
        });
        assignResponse.EnsureSuccessStatusCode();
        return created;
    }

    private sealed record CreateTicketResult(Guid Id, string TicketNumber, string Status, string Priority);
    private sealed record PagedAuditListResponse(IReadOnlyList<AuditListItemResponse> Data, int Page, int PageSize, int TotalCount, int TotalPages);
    private sealed record AuditListItemResponse(Guid Id, Guid? ActorUserId, string? ActorDisplayName, string Action, string EntityType, Guid EntityId, DateTime CreatedAt, string? CorrelationId);
    private sealed record AuditLogDetailResponse(Guid Id, Guid? ActorUserId, string? ActorDisplayName, string Action, string EntityType, Guid EntityId, string? PreviousValue, string? NewValue, string? CorrelationId, DateTime CreatedAt);
}
