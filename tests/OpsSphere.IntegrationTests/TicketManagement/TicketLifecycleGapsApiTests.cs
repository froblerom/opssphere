using System.Net;
using System.Net.Http.Json;
using OpsSphere.Infrastructure.Persistence.SeedData;
using OpsSphere.IntegrationTests.TestInfrastructure;

namespace OpsSphere.IntegrationTests.TicketManagement;

public sealed class TicketLifecycleGapsApiTests
{
    private const string AgentEmail = "agent.novabank@opssphere.local";
    private const string SupervisorEmail = "supervisor.novabank@opssphere.local";
    private const string ViewerEmail = "viewer.latam@opssphere.local";

    [Fact]
    public async Task UpdatePriority_Supervisor_CanChangePriority_Returns200()
    {
        await using var factory = await OpsSphereSqliteFactory.CreateAsync();
        var supervisor = await factory.CreateAuthenticatedClientAsync(SupervisorEmail);

        var created = await CreateTicketAsync(supervisor);
        var response = await supervisor.PutAsJsonAsync($"/api/tickets/{created.Id}/priority", new
        {
            priority = "High",
            changeReason = "Escalating priority for test"
        });

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var result = await OpsSphereSqliteFactory.ReadDataAsync<UpdateTicketPriorityResponse>(response);
        Assert.Equal("Normal", result.PreviousPriority);
        Assert.Equal("High", result.NewPriority);
    }

    [Fact]
    public async Task UpdatePriority_Agent_CanChangePriority_Returns200()
    {
        await using var factory = await OpsSphereSqliteFactory.CreateAsync();
        var agent = await factory.CreateAuthenticatedClientAsync(AgentEmail);

        var created = await CreateTicketAsync(agent);
        var response = await agent.PutAsJsonAsync($"/api/tickets/{created.Id}/priority", new
        {
            priority = "Critical",
            changeReason = "Critical escalation for test"
        });

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var result = await OpsSphereSqliteFactory.ReadDataAsync<UpdateTicketPriorityResponse>(response);
        Assert.Equal("Critical", result.NewPriority);
    }

    [Fact]
    public async Task UpdatePriority_Viewer_Returns403()
    {
        await using var factory = await OpsSphereSqliteFactory.CreateAsync();
        var agent = await factory.CreateAuthenticatedClientAsync(AgentEmail);
        var viewer = await factory.CreateAuthenticatedClientAsync(ViewerEmail);

        var created = await CreateTicketAsync(agent);
        var response = await viewer.PutAsJsonAsync($"/api/tickets/{created.Id}/priority", new
        {
            priority = "High",
            changeReason = "Viewer attempt"
        });

        Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);
    }

    [Fact]
    public async Task UpdatePriority_WithInvalidPriority_Returns400()
    {
        await using var factory = await OpsSphereSqliteFactory.CreateAsync();
        var agent = await factory.CreateAuthenticatedClientAsync(AgentEmail);

        var created = await CreateTicketAsync(agent);
        var response = await agent.PutAsJsonAsync($"/api/tickets/{created.Id}/priority", new
        {
            priority = "Urgent",
            changeReason = "Invalid priority test"
        });

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        var error = await OpsSphereSqliteFactory.ReadErrorAsync(response);
        Assert.Equal("validation_error", error.Error.Code);
    }

    [Fact]
    public async Task GetEscalationQueue_WithoutAuth_Returns401()
    {
        await using var factory = await OpsSphereSqliteFactory.CreateAsync();

        var response = await factory.CreateClient().GetAsync("/api/tickets/escalations");

        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    [Fact]
    public async Task GetEscalationQueue_Supervisor_ReturnsSeededEscalation()
    {
        await using var factory = await OpsSphereSqliteFactory.CreateAsync();
        var supervisor = await factory.CreateAuthenticatedClientAsync(SupervisorEmail);

        var response = await supervisor.GetAsync("/api/tickets/escalations");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var items = await OpsSphereSqliteFactory.ReadDataAsync<IReadOnlyList<EscalationQueueItemDto>>(response);
        Assert.NotEmpty(items);
        Assert.Contains(items, item => item.TicketId == SeedIds.Tickets.NovaBankEscalated);
    }

    [Fact]
    public async Task GetEscalationQueue_Agent_ReturnsOnly_CampaignScopedEscalations()
    {
        await using var factory = await OpsSphereSqliteFactory.CreateAsync();
        var agent = await factory.CreateAuthenticatedClientAsync(AgentEmail);

        var response = await agent.GetAsync("/api/tickets/escalations");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var items = await OpsSphereSqliteFactory.ReadDataAsync<IReadOnlyList<EscalationQueueItemDto>>(response);
        Assert.All(items, item => Assert.Equal("Credit Card Support", item.CampaignName));
    }

    [Fact]
    public async Task GetTicketHistory_WithoutAuth_Returns401()
    {
        await using var factory = await OpsSphereSqliteFactory.CreateAsync();

        var response = await factory.CreateClient().GetAsync($"/api/tickets/{SeedIds.Tickets.NovaBankOpen}/history");

        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    [Fact]
    public async Task GetTicketHistory_ReturnsSeededHistoryForOpenTicket()
    {
        await using var factory = await OpsSphereSqliteFactory.CreateAsync();
        var agent = await factory.CreateAuthenticatedClientAsync(AgentEmail);

        var response = await agent.GetAsync($"/api/tickets/{SeedIds.Tickets.NovaBankOpen}/history");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var history = await OpsSphereSqliteFactory.ReadDataAsync<IReadOnlyList<TicketStatusHistoryItemDto>>(response);
        Assert.NotEmpty(history);
        Assert.Contains(history, h => h.NewStatus == "Open");
    }

    [Fact]
    public async Task GetTicketHistory_AfterStatusUpdate_IncludesNewEntry()
    {
        await using var factory = await OpsSphereSqliteFactory.CreateAsync();
        var supervisor = await factory.CreateAuthenticatedClientAsync(SupervisorEmail);

        var created = await CreateTicketAsync(supervisor);
        var assignResponse = await supervisor.PostAsJsonAsync($"/api/tickets/{created.Id}/assign", new
        {
            targetAgentUserId = SeedIds.Users.AgentNovabank,
            reassignmentReason = (string?)null
        });
        assignResponse.EnsureSuccessStatusCode();

        var historyResponse = await supervisor.GetAsync($"/api/tickets/{created.Id}/history");
        Assert.Equal(HttpStatusCode.OK, historyResponse.StatusCode);
        var history = await OpsSphereSqliteFactory.ReadDataAsync<IReadOnlyList<TicketStatusHistoryItemDto>>(historyResponse);

        Assert.True(history.Count >= 2);
        Assert.Contains(history, h => h.NewStatus == "Open");
        Assert.Contains(history, h => h.NewStatus == "Assigned");
    }

    [Fact]
    public async Task GetTicketComments_WithoutAuth_Returns401()
    {
        await using var factory = await OpsSphereSqliteFactory.CreateAsync();

        var response = await factory.CreateClient().GetAsync($"/api/tickets/{SeedIds.Tickets.NovaBankInProgress}/comments");

        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    [Fact]
    public async Task GetTicketComments_ReturnsSeededCommentForInProgressTicket()
    {
        await using var factory = await OpsSphereSqliteFactory.CreateAsync();
        var supervisor = await factory.CreateAuthenticatedClientAsync(SupervisorEmail);

        var response = await supervisor.GetAsync($"/api/tickets/{SeedIds.Tickets.NovaBankInProgress}/comments");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var comments = await OpsSphereSqliteFactory.ReadDataAsync<IReadOnlyList<TicketCommentDto>>(response);
        Assert.NotEmpty(comments);
        Assert.All(comments, c => Assert.Equal(SeedIds.Tickets.NovaBankInProgress, c.TicketId));
    }

    [Fact]
    public async Task GetTicketComments_AfterAddComment_IncludesNewComment()
    {
        await using var factory = await OpsSphereSqliteFactory.CreateAsync();
        var supervisor = await factory.CreateAuthenticatedClientAsync(SupervisorEmail);

        var created = await CreateTicketAsync(supervisor);
        var addResponse = await supervisor.PostAsJsonAsync($"/api/tickets/{created.Id}/comments", new
        {
            body = "Lifecycle gap test comment body"
        });
        addResponse.EnsureSuccessStatusCode();

        var getResponse = await supervisor.GetAsync($"/api/tickets/{created.Id}/comments");
        Assert.Equal(HttpStatusCode.OK, getResponse.StatusCode);
        var comments = await OpsSphereSqliteFactory.ReadDataAsync<IReadOnlyList<TicketCommentDto>>(getResponse);

        Assert.Single(comments);
        Assert.Equal("Lifecycle gap test comment body", comments[0].Body);
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
            subject = "Lifecycle gap test ticket",
            description = "Testing lifecycle gaps in integration coverage"
        });
        Assert.Equal(HttpStatusCode.Created, response.StatusCode);
        return await OpsSphereSqliteFactory.ReadDataAsync<CreateTicketResult>(response);
    }

    private sealed record CreateTicketResult(Guid Id, string TicketNumber, string Status, string Priority);
    private sealed record UpdateTicketPriorityResponse(Guid TicketId, string TicketNumber, string PreviousPriority, string NewPriority, string Message);
    private sealed record EscalationQueueItemDto(Guid EscalationId, Guid TicketId, string TicketNumber, string CustomerName, string AccountName, string CampaignName, string Priority, string Status, string SlaState, DateTime EscalatedAt, Guid EscalatedByUserId, string EscalatedByName, string EscalationReason);
    private sealed record TicketStatusHistoryItemDto(Guid Id, Guid TicketId, string? PreviousStatus, string NewStatus, Guid ChangedByUserId, string? ChangeReason, DateTime CreatedAt);
    private sealed record TicketCommentDto(Guid Id, Guid TicketId, Guid AuthorUserId, string AuthorDisplayName, string Body, DateTime CreatedAt);
}
