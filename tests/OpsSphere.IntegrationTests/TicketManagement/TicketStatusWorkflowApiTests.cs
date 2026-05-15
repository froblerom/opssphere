using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text.Json;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.AspNetCore.TestHost;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using OpsSphere.Domain.Entities;
using OpsSphere.Domain.Enums;
using OpsSphere.Infrastructure.Persistence;
using OpsSphere.Infrastructure.Persistence.SeedData;

namespace OpsSphere.IntegrationTests.TicketManagement;

public sealed class TicketStatusWorkflowApiTests
{
    private const string AdminEmail = "admin@opssphere.local";
    private const string AgentEmail = "agent.novabank@opssphere.local";
    private const string SupervisorEmail = "supervisor.novabank@opssphere.local";
    private const string ViewerEmail = "viewer.latam@opssphere.local";
    private const string ManagerEmail = "manager.latam@opssphere.local";

    private static readonly JsonSerializerOptions JsonOptions = new(JsonSerializerDefaults.Web);

    [Fact]
    public async Task Agent_CanUpdateStatus_OpenToInProgress_Returns200()
    {
        await using var factory = await TicketStatusWorkflowApiFactory.CreateAsync();
        var agent = await CreateAuthenticatedClientAsync(factory, AgentEmail);
        var ticket = await CreateNovaBankTicketAsync(agent, "Agent status update");

        var response = await UpdateStatusAsync(agent, ticket.Id, "InProgress", "Starting work");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var result = await ReadDataAsync<UpdateTicketStatusResponse>(response);
        Assert.Equal(ticket.Id, result.TicketId);
        Assert.Equal(ticket.TicketNumber, result.TicketNumber);
        Assert.Equal("Open", result.PreviousStatus);
        Assert.Equal("InProgress", result.NewStatus);
        Assert.Equal("Ticket status updated successfully.", result.Message);
    }

    [Fact]
    public async Task Supervisor_CanUpdateStatus_AssignedToInProgress_Returns200()
    {
        await using var factory = await TicketStatusWorkflowApiFactory.CreateAsync();
        var supervisor = await CreateAuthenticatedClientAsync(factory, SupervisorEmail);
        var ticket = await CreateNovaBankTicketAsync(supervisor, "Supervisor status update");
        await AssignTicketAsync(supervisor, ticket.Id, SeedIds.Users.AgentNovabank);

        var response = await UpdateStatusAsync(supervisor, ticket.Id, "InProgress");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var result = await ReadDataAsync<UpdateTicketStatusResponse>(response);
        Assert.Equal("Assigned", result.PreviousStatus);
        Assert.Equal("InProgress", result.NewStatus);
    }

    [Fact]
    public async Task Agent_CanUpdateStatus_InProgressToWaitingForCustomer_Returns200()
    {
        await using var factory = await TicketStatusWorkflowApiFactory.CreateAsync();
        var agent = await CreateAuthenticatedClientAsync(factory, AgentEmail);
        var ticket = await CreateNovaBankTicketAsync(agent, "Agent waiting status update");
        await UpdateStatusAsync(agent, ticket.Id, "InProgress");

        var response = await UpdateStatusAsync(agent, ticket.Id, "WaitingForCustomer", "Need customer response");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var result = await ReadDataAsync<UpdateTicketStatusResponse>(response);
        Assert.Equal("InProgress", result.PreviousStatus);
        Assert.Equal("WaitingForCustomer", result.NewStatus);
    }

    [Fact]
    public async Task Admin_CannotUpdateStatus_Returns403()
    {
        await using var factory = await TicketStatusWorkflowApiFactory.CreateAsync();
        var supervisor = await CreateAuthenticatedClientAsync(factory, SupervisorEmail);
        var admin = await CreateAuthenticatedClientAsync(factory, AdminEmail);
        var ticket = await CreateNovaBankTicketAsync(supervisor, "Admin status forbidden");

        var response = await UpdateStatusAsync(admin, ticket.Id, "InProgress");

        Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);
    }

    [Fact]
    public async Task Viewer_CannotUpdateStatus_Returns403()
    {
        await using var factory = await TicketStatusWorkflowApiFactory.CreateAsync();
        var supervisor = await CreateAuthenticatedClientAsync(factory, SupervisorEmail);
        var viewer = await CreateAuthenticatedClientAsync(factory, ViewerEmail);
        var ticket = await CreateNovaBankTicketAsync(supervisor, "Viewer status forbidden");

        var response = await UpdateStatusAsync(viewer, ticket.Id, "InProgress");

        Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);
    }

    [Fact]
    public async Task Manager_CannotUpdateStatus_Returns403()
    {
        await using var factory = await TicketStatusWorkflowApiFactory.CreateAsync();
        var supervisor = await CreateAuthenticatedClientAsync(factory, SupervisorEmail);
        var manager = await CreateAuthenticatedClientAsync(factory, ManagerEmail);
        var ticket = await CreateNovaBankTicketAsync(supervisor, "Manager status forbidden");

        var response = await UpdateStatusAsync(manager, ticket.Id, "InProgress");

        Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);
    }

    [Fact]
    public async Task UpdateStatus_CrossScope_Returns404()
    {
        await using var factory = await TicketStatusWorkflowApiFactory.CreateAsync();
        var supervisor = await CreateAuthenticatedClientAsync(factory, SupervisorEmail);
        var outOfScopeTicketId = await AddTicketDirectlyAsync(
            factory,
            SeedIds.Accounts.Streamly,
            SeedIds.Campaigns.StreamlyCreator,
            SeedIds.Customers.StreamlyCustomer1,
            TicketStatus.Open,
            "Out-of-scope status update");

        var response = await UpdateStatusAsync(supervisor, outOfScopeTicketId, "InProgress");

        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }

    [Fact]
    public async Task UpdateStatus_ClosedTicket_Returns400()
    {
        await using var factory = await TicketStatusWorkflowApiFactory.CreateAsync();
        var agent = await CreateAuthenticatedClientAsync(factory, AgentEmail);
        var ticket = await CreateNovaBankTicketAsync(agent, "Closed status update");
        await SetTicketStatusAsync(factory, ticket.Id, TicketStatus.Closed);

        var response = await UpdateStatusAsync(agent, ticket.Id, "InProgress");

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        var error = await ReadErrorAsync(response);
        Assert.Equal("business_rule_violation", error.Error.Code);
        Assert.Contains("closed", error.Error.Message, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public async Task UpdateStatus_SameStatus_Returns400()
    {
        await using var factory = await TicketStatusWorkflowApiFactory.CreateAsync();
        var agent = await CreateAuthenticatedClientAsync(factory, AgentEmail);
        var ticket = await CreateNovaBankTicketAsync(agent, "Same status update");

        var response = await UpdateStatusAsync(agent, ticket.Id, "Open");

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        var error = await ReadErrorAsync(response);
        Assert.Equal("business_rule_violation", error.Error.Code);
        Assert.Contains("already", error.Error.Message, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public async Task UpdateStatus_InvalidTransition_Returns400()
    {
        await using var factory = await TicketStatusWorkflowApiFactory.CreateAsync();
        var agent = await CreateAuthenticatedClientAsync(factory, AgentEmail);
        var ticket = await CreateNovaBankTicketAsync(agent, "Invalid status transition");

        var response = await UpdateStatusAsync(agent, ticket.Id, "WaitingForCustomer");

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        var error = await ReadErrorAsync(response);
        Assert.Equal("business_rule_violation", error.Error.Code);
        Assert.Contains("Cannot transition", error.Error.Message, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public async Task UpdateStatus_DestinationEscalated_Returns400() =>
        await AssertBlockedDestinationAsync("Escalated");

    [Fact]
    public async Task UpdateStatus_DestinationResolved_Returns400() =>
        await AssertBlockedDestinationAsync("Resolved");

    [Fact]
    public async Task UpdateStatus_DestinationClosed_Returns400() =>
        await AssertBlockedDestinationAsync("Closed");

    [Fact]
    public async Task UpdateStatus_AssignedWithoutAgent_Returns400()
    {
        await using var factory = await TicketStatusWorkflowApiFactory.CreateAsync();
        var agent = await CreateAuthenticatedClientAsync(factory, AgentEmail);
        var ticket = await CreateNovaBankTicketAsync(agent, "Assigned without agent");

        var response = await UpdateStatusAsync(agent, ticket.Id, "Assigned");

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        var error = await ReadErrorAsync(response);
        Assert.Equal("business_rule_violation", error.Error.Code);
        Assert.Contains("no agent", error.Error.Message, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public async Task UpdateStatus_InvalidStatusString_Returns400()
    {
        await using var factory = await TicketStatusWorkflowApiFactory.CreateAsync();
        var agent = await CreateAuthenticatedClientAsync(factory, AgentEmail);
        var ticket = await CreateNovaBankTicketAsync(agent, "Invalid status string");

        var response = await UpdateStatusAsync(agent, ticket.Id, "TotallyInvalid");

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        var error = await ReadErrorAsync(response);
        Assert.Equal("validation_error", error.Error.Code);
        Assert.Contains(error.Error.Details ?? [], d => d.Field == "status");
    }

    [Fact]
    public async Task UpdateStatus_CreatesStatusHistoryRecord()
    {
        await using var factory = await TicketStatusWorkflowApiFactory.CreateAsync();
        var agent = await CreateAuthenticatedClientAsync(factory, AgentEmail);
        var ticket = await CreateNovaBankTicketAsync(agent, "Status history update");

        await UpdateStatusAsync(agent, ticket.Id, "InProgress", "Investigating account details");

        using var scope = factory.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<OpsSphereDbContext>();
        var history = await db.TicketStatusHistory
            .AsNoTracking()
            .Where(h => h.TicketId == ticket.Id && h.PreviousStatus == "Open" && h.NewStatus == "InProgress")
            .SingleOrDefaultAsync();

        Assert.NotNull(history);
        Assert.Equal(SeedIds.Users.AgentNovabank, history.ChangedByUserId);
        Assert.Equal("Investigating account details", history.ChangeReason);
    }

    [Fact]
    public async Task UpdateStatus_CreatesAuditLogRecord()
    {
        await using var factory = await TicketStatusWorkflowApiFactory.CreateAsync();
        var agent = await CreateAuthenticatedClientAsync(factory, AgentEmail);
        var ticket = await CreateNovaBankTicketAsync(agent, "Status audit update");

        await UpdateStatusAsync(agent, ticket.Id, "InProgress");

        using var scope = factory.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<OpsSphereDbContext>();
        var audit = await db.AuditLogs
            .AsNoTracking()
            .SingleOrDefaultAsync(log => log.Action == "TicketStatusChanged" && log.EntityType == "Ticket" && log.EntityId == ticket.Id);

        Assert.NotNull(audit);
        Assert.False(string.IsNullOrWhiteSpace(audit.CorrelationId));
        Assert.Contains("Open", $"{audit.PreviousValue}{audit.NewValue}", StringComparison.OrdinalIgnoreCase);
        Assert.Contains("InProgress", $"{audit.PreviousValue}{audit.NewValue}", StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public async Task UpdateStatus_GetTicket_ReflectsNewStatus()
    {
        await using var factory = await TicketStatusWorkflowApiFactory.CreateAsync();
        var agent = await CreateAuthenticatedClientAsync(factory, AgentEmail);
        var ticket = await CreateNovaBankTicketAsync(agent, "Status detail reflection");

        await UpdateStatusAsync(agent, ticket.Id, "InProgress");
        var getResponse = await agent.GetAsync($"/api/tickets/{ticket.Id}");

        Assert.Equal(HttpStatusCode.OK, getResponse.StatusCode);
        var detail = await ReadDataAsync<TicketDetailDto>(getResponse);
        Assert.Equal("InProgress", detail.Status);
    }

    [Fact]
    public async Task UpdateStatus_BusinessRuleError_UsesCanonicalEnvelope()
    {
        await using var factory = await TicketStatusWorkflowApiFactory.CreateAsync();
        var agent = await CreateAuthenticatedClientAsync(factory, AgentEmail);
        var ticket = await CreateNovaBankTicketAsync(agent, "Status canonical envelope");

        var response = await UpdateStatusAsync(agent, ticket.Id, "WaitingForCustomer");

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        var error = await ReadErrorAsync(response);
        Assert.Equal("business_rule_violation", error.Error.Code);
        Assert.False(string.IsNullOrWhiteSpace(error.Error.Message));
        Assert.False(string.IsNullOrWhiteSpace(error.Error.CorrelationId));
    }

    [Fact]
    public async Task Agent_CanUpdatePriority_NormalToHigh_Returns200()
    {
        await using var factory = await TicketStatusWorkflowApiFactory.CreateAsync();
        var agent = await CreateAuthenticatedClientAsync(factory, AgentEmail);
        var ticket = await CreateNovaBankTicketAsync(agent, "Agent priority update");

        var response = await UpdatePriorityAsync(agent, ticket.Id, "High", "Raising urgency");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var result = await ReadDataAsync<UpdateTicketPriorityResponse>(response);
        Assert.Equal(ticket.Id, result.TicketId);
        Assert.Equal(ticket.TicketNumber, result.TicketNumber);
        Assert.Equal("Normal", result.PreviousPriority);
        Assert.Equal("High", result.NewPriority);
        Assert.Equal("Ticket priority updated successfully.", result.Message);
    }

    [Fact]
    public async Task Supervisor_CanUpdatePriority_Returns200()
    {
        await using var factory = await TicketStatusWorkflowApiFactory.CreateAsync();
        var supervisor = await CreateAuthenticatedClientAsync(factory, SupervisorEmail);
        var ticket = await CreateNovaBankTicketAsync(supervisor, "Supervisor priority update");

        var response = await UpdatePriorityAsync(supervisor, ticket.Id, "Critical");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var result = await ReadDataAsync<UpdateTicketPriorityResponse>(response);
        Assert.Equal("Normal", result.PreviousPriority);
        Assert.Equal("Critical", result.NewPriority);
    }

    [Fact]
    public async Task Admin_CannotUpdatePriority_Returns403()
    {
        await using var factory = await TicketStatusWorkflowApiFactory.CreateAsync();
        var supervisor = await CreateAuthenticatedClientAsync(factory, SupervisorEmail);
        var admin = await CreateAuthenticatedClientAsync(factory, AdminEmail);
        var ticket = await CreateNovaBankTicketAsync(supervisor, "Admin priority forbidden");

        var response = await UpdatePriorityAsync(admin, ticket.Id, "High");

        Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);
    }

    [Fact]
    public async Task Viewer_CannotUpdatePriority_Returns403()
    {
        await using var factory = await TicketStatusWorkflowApiFactory.CreateAsync();
        var supervisor = await CreateAuthenticatedClientAsync(factory, SupervisorEmail);
        var viewer = await CreateAuthenticatedClientAsync(factory, ViewerEmail);
        var ticket = await CreateNovaBankTicketAsync(supervisor, "Viewer priority forbidden");

        var response = await UpdatePriorityAsync(viewer, ticket.Id, "High");

        Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);
    }

    [Fact]
    public async Task Manager_CannotUpdatePriority_Returns403()
    {
        await using var factory = await TicketStatusWorkflowApiFactory.CreateAsync();
        var supervisor = await CreateAuthenticatedClientAsync(factory, SupervisorEmail);
        var manager = await CreateAuthenticatedClientAsync(factory, ManagerEmail);
        var ticket = await CreateNovaBankTicketAsync(supervisor, "Manager priority forbidden");

        var response = await UpdatePriorityAsync(manager, ticket.Id, "High");

        Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);
    }

    [Fact]
    public async Task UpdatePriority_CrossScope_Returns404()
    {
        await using var factory = await TicketStatusWorkflowApiFactory.CreateAsync();
        var supervisor = await CreateAuthenticatedClientAsync(factory, SupervisorEmail);
        var outOfScopeTicketId = await AddTicketDirectlyAsync(
            factory,
            SeedIds.Accounts.Streamly,
            SeedIds.Campaigns.StreamlyCreator,
            SeedIds.Customers.StreamlyCustomer1,
            TicketStatus.Open,
            "Out-of-scope priority update");

        var response = await UpdatePriorityAsync(supervisor, outOfScopeTicketId, "High");

        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }

    [Fact]
    public async Task UpdatePriority_ClosedTicket_Returns400()
    {
        await using var factory = await TicketStatusWorkflowApiFactory.CreateAsync();
        var agent = await CreateAuthenticatedClientAsync(factory, AgentEmail);
        var ticket = await CreateNovaBankTicketAsync(agent, "Closed priority update");
        await SetTicketStatusAsync(factory, ticket.Id, TicketStatus.Closed);

        var response = await UpdatePriorityAsync(agent, ticket.Id, "High");

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        var error = await ReadErrorAsync(response);
        Assert.Equal("business_rule_violation", error.Error.Code);
        Assert.Contains("closed", error.Error.Message, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public async Task UpdatePriority_SamePriority_Returns400()
    {
        await using var factory = await TicketStatusWorkflowApiFactory.CreateAsync();
        var agent = await CreateAuthenticatedClientAsync(factory, AgentEmail);
        var ticket = await CreateNovaBankTicketAsync(agent, "Same priority update");

        var response = await UpdatePriorityAsync(agent, ticket.Id, "Normal");

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        var error = await ReadErrorAsync(response);
        Assert.Equal("business_rule_violation", error.Error.Code);
        Assert.Contains("already", error.Error.Message, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public async Task UpdatePriority_InvalidPriorityString_Returns400()
    {
        await using var factory = await TicketStatusWorkflowApiFactory.CreateAsync();
        var agent = await CreateAuthenticatedClientAsync(factory, AgentEmail);
        var ticket = await CreateNovaBankTicketAsync(agent, "Invalid priority string");

        var response = await UpdatePriorityAsync(agent, ticket.Id, "TotallyInvalid");

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        var error = await ReadErrorAsync(response);
        Assert.Equal("validation_error", error.Error.Code);
        Assert.Contains(error.Error.Details ?? [], d => d.Field == "priority");
    }

    [Fact]
    public async Task UpdatePriority_CreatesAuditLogRecord()
    {
        await using var factory = await TicketStatusWorkflowApiFactory.CreateAsync();
        var agent = await CreateAuthenticatedClientAsync(factory, AgentEmail);
        var ticket = await CreateNovaBankTicketAsync(agent, "Priority audit update");

        await UpdatePriorityAsync(agent, ticket.Id, "High");

        using var scope = factory.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<OpsSphereDbContext>();
        var audit = await db.AuditLogs
            .AsNoTracking()
            .SingleOrDefaultAsync(log => log.Action == "TicketPriorityChanged" && log.EntityType == "Ticket" && log.EntityId == ticket.Id);

        Assert.NotNull(audit);
        Assert.False(string.IsNullOrWhiteSpace(audit.CorrelationId));
        Assert.Contains("Normal", $"{audit.PreviousValue}{audit.NewValue}", StringComparison.OrdinalIgnoreCase);
        Assert.Contains("High", $"{audit.PreviousValue}{audit.NewValue}", StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public async Task UpdatePriority_DoesNotRecalculateSla()
    {
        await using var factory = await TicketStatusWorkflowApiFactory.CreateAsync();
        var agent = await CreateAuthenticatedClientAsync(factory, AgentEmail);
        var ticket = await CreateNovaBankTicketAsync(agent, "Priority SLA unchanged");
        var before = await GetSlaSnapshotAsync(factory, ticket.Id);

        await UpdatePriorityAsync(agent, ticket.Id, "Critical");

        var after = await GetSlaSnapshotAsync(factory, ticket.Id);
        Assert.Equal(before.TicketSlaDueAt, after.TicketSlaDueAt);
        Assert.Equal(before.SlaStateDueAt, after.SlaStateDueAt);
        Assert.Equal(before.SlaState, after.SlaState);
        Assert.Equal(before.LastEvaluatedAt, after.LastEvaluatedAt);
    }

    [Fact]
    public async Task UpdatePriority_GetTicket_ReflectsNewPriority()
    {
        await using var factory = await TicketStatusWorkflowApiFactory.CreateAsync();
        var agent = await CreateAuthenticatedClientAsync(factory, AgentEmail);
        var ticket = await CreateNovaBankTicketAsync(agent, "Priority detail reflection");

        await UpdatePriorityAsync(agent, ticket.Id, "High");
        var getResponse = await agent.GetAsync($"/api/tickets/{ticket.Id}");

        Assert.Equal(HttpStatusCode.OK, getResponse.StatusCode);
        var detail = await ReadDataAsync<TicketDetailDto>(getResponse);
        Assert.Equal("High", detail.Priority);
    }

    [Fact]
    public async Task UpdatePriority_BusinessRuleError_UsesCanonicalEnvelope()
    {
        await using var factory = await TicketStatusWorkflowApiFactory.CreateAsync();
        var agent = await CreateAuthenticatedClientAsync(factory, AgentEmail);
        var ticket = await CreateNovaBankTicketAsync(agent, "Priority canonical envelope");

        var response = await UpdatePriorityAsync(agent, ticket.Id, "Normal");

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        var error = await ReadErrorAsync(response);
        Assert.Equal("business_rule_violation", error.Error.Code);
        Assert.False(string.IsNullOrWhiteSpace(error.Error.Message));
        Assert.False(string.IsNullOrWhiteSpace(error.Error.CorrelationId));
    }

    [Fact]
    public async Task StatusAuditPayload_DoesNotContainSubjectDescriptionChangeReasonCustomerDataOrPii()
    {
        await using var factory = await TicketStatusWorkflowApiFactory.CreateAsync();
        var agent = await CreateAuthenticatedClientAsync(factory, AgentEmail);
        const string sensitiveSubject = "StatusSensitiveSubject-ABC123";
        const string sensitiveDescription = "StatusSensitiveDescription-XYZ987";
        const string sensitiveReason = "StatusSensitiveChangeReason-SECRET456";
        var ticket = await CreateNovaBankTicketAsync(agent, sensitiveSubject, sensitiveDescription);

        await UpdateStatusAsync(agent, ticket.Id, "InProgress", sensitiveReason);

        var combined = await GetAuditPayloadAsync(factory, "TicketStatusChanged", ticket.Id);
        AssertAuditPayloadIsSafe(combined, sensitiveSubject, sensitiveDescription, sensitiveReason);
    }

    [Fact]
    public async Task PriorityAuditPayload_DoesNotContainSubjectDescriptionChangeReasonCustomerDataOrPii()
    {
        await using var factory = await TicketStatusWorkflowApiFactory.CreateAsync();
        var agent = await CreateAuthenticatedClientAsync(factory, AgentEmail);
        const string sensitiveSubject = "PrioritySensitiveSubject-ABC123";
        const string sensitiveDescription = "PrioritySensitiveDescription-XYZ987";
        const string sensitiveReason = "PrioritySensitiveChangeReason-SECRET456";
        var ticket = await CreateNovaBankTicketAsync(agent, sensitiveSubject, sensitiveDescription);

        await UpdatePriorityAsync(agent, ticket.Id, "High", sensitiveReason);

        var combined = await GetAuditPayloadAsync(factory, "TicketPriorityChanged", ticket.Id);
        AssertAuditPayloadIsSafe(combined, sensitiveSubject, sensitiveDescription, sensitiveReason);
    }

    private static async Task AssertBlockedDestinationAsync(string status)
    {
        await using var factory = await TicketStatusWorkflowApiFactory.CreateAsync();
        var agent = await CreateAuthenticatedClientAsync(factory, AgentEmail);
        var ticket = await CreateNovaBankTicketAsync(agent, $"Blocked destination {status}");

        var response = await UpdateStatusAsync(agent, ticket.Id, status);

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        var error = await ReadErrorAsync(response);
        Assert.Equal("business_rule_violation", error.Error.Code);
        Assert.Contains("Only Assigned, InProgress, and WaitingForCustomer", error.Error.Message, StringComparison.OrdinalIgnoreCase);
    }

    private static Task<HttpResponseMessage> UpdateStatusAsync(
        HttpClient client,
        Guid ticketId,
        string status,
        string? changeReason = null)
    {
        return client.PutAsJsonAsync($"/api/tickets/{ticketId}/status", new
        {
            status,
            changeReason
        });
    }

    private static Task<HttpResponseMessage> UpdatePriorityAsync(
        HttpClient client,
        Guid ticketId,
        string priority,
        string? changeReason = null)
    {
        return client.PutAsJsonAsync($"/api/tickets/{ticketId}/priority", new
        {
            priority,
            changeReason
        });
    }

    private static Task<HttpResponseMessage> AssignTicketAsync(
        HttpClient client,
        Guid ticketId,
        Guid targetAgentUserId)
    {
        return client.PostAsJsonAsync($"/api/tickets/{ticketId}/assign", new
        {
            targetAgentUserId,
            reassignmentReason = (string?)null
        });
    }

    private static async Task<CreateTicketResult> CreateNovaBankTicketAsync(
        HttpClient client,
        string subject,
        string? description = null) =>
        await CreateTicketAsync(
            client,
            SeedIds.Customers.NovaBankCustomer1,
            SeedIds.Accounts.NovaBank,
            SeedIds.Campaigns.NovaBankCreditCard,
            "Support",
            "Normal",
            subject,
            description ?? $"Description for {subject}");

    private static async Task<CreateTicketResult> CreateTicketAsync(
        HttpClient client,
        Guid customerId,
        Guid accountId,
        Guid campaignId,
        string category,
        string priority,
        string subject,
        string description)
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
        return await ReadDataAsync<CreateTicketResult>(response);
    }

    private static async Task<Guid> AddTicketDirectlyAsync(
        TicketStatusWorkflowApiFactory factory,
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

    private static async Task SetTicketStatusAsync(TicketStatusWorkflowApiFactory factory, Guid ticketId, TicketStatus status)
    {
        using var scope = factory.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<OpsSphereDbContext>();
        var ticket = await db.Tickets.SingleAsync(t => t.Id == ticketId);
        ticket.Status = status;
        await db.SaveChangesAsync();
    }

    private static async Task<SlaSnapshot> GetSlaSnapshotAsync(TicketStatusWorkflowApiFactory factory, Guid ticketId)
    {
        using var scope = factory.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<OpsSphereDbContext>();
        var ticket = await db.Tickets.AsNoTracking().SingleAsync(t => t.Id == ticketId);
        var slaState = await db.TicketSlaStates.AsNoTracking().SingleAsync(s => s.TicketId == ticketId);
        return new SlaSnapshot(ticket.SlaDueAt, slaState.DueAt, slaState.State, slaState.LastEvaluatedAt);
    }

    private static async Task<string> GetAuditPayloadAsync(TicketStatusWorkflowApiFactory factory, string action, Guid ticketId)
    {
        using var scope = factory.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<OpsSphereDbContext>();
        var audit = await db.AuditLogs
            .AsNoTracking()
            .SingleAsync(log => log.Action == action && log.EntityType == "Ticket" && log.EntityId == ticketId);

        return $"{audit.PreviousValue}{audit.NewValue}";
    }

    private static void AssertAuditPayloadIsSafe(string combined, params string[] sensitiveValues)
    {
        foreach (var sensitiveValue in sensitiveValues)
            Assert.DoesNotContain(sensitiveValue, combined, StringComparison.OrdinalIgnoreCase);

        Assert.DoesNotContain("Carlos", combined, StringComparison.OrdinalIgnoreCase);
        Assert.DoesNotContain("Mendez", combined, StringComparison.OrdinalIgnoreCase);
        Assert.DoesNotContain("carlos.mendez", combined, StringComparison.OrdinalIgnoreCase);
        Assert.DoesNotContain("+52-55", combined, StringComparison.OrdinalIgnoreCase);
        Assert.DoesNotContain("Diego", combined, StringComparison.OrdinalIgnoreCase);
        Assert.DoesNotContain("Santos", combined, StringComparison.OrdinalIgnoreCase);
        Assert.DoesNotContain(AgentEmail, combined, StringComparison.OrdinalIgnoreCase);
    }

    private static async Task<HttpClient> CreateAuthenticatedClientAsync(TicketStatusWorkflowApiFactory factory, string email)
    {
        var client = factory.CreateClient();
        var loginResponse = await client.PostAsJsonAsync("/api/auth/login", new
        {
            email,
            password = OpsSphereSeedData.LocalDemoPassword
        });
        loginResponse.EnsureSuccessStatusCode();
        var loginBody = await ReadResponseAsync<ApiResponse<LoginData>>(loginResponse);
        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", loginBody.Data.AccessToken);
        return client;
    }

    private static async Task<T> ReadDataAsync<T>(HttpResponseMessage response)
    {
        var envelope = await ReadResponseAsync<ApiResponse<T>>(response);
        return envelope.Data;
    }

    private static async Task<ApiErrorResponse> ReadErrorAsync(HttpResponseMessage response) =>
        await ReadResponseAsync<ApiErrorResponse>(response);

    private static async Task<T> ReadResponseAsync<T>(HttpResponseMessage response)
    {
        var json = await response.Content.ReadAsStringAsync();
        var value = JsonSerializer.Deserialize<T>(json, JsonOptions);
        return value ?? throw new InvalidOperationException($"Could not deserialize {typeof(T).Name} from {(int)response.StatusCode}: {json}");
    }

    private sealed record ApiResponse<T>(T Data);
    private sealed record ApiErrorResponse(ApiError Error);
    private sealed record ApiError(string Code, string Message, IReadOnlyList<ApiErrorDetail>? Details, string? CorrelationId);
    private sealed record ApiErrorDetail(string? Field, string Message);
    private sealed record LoginData(string AccessToken);

    private sealed record CreateTicketResult(
        Guid Id,
        string TicketNumber,
        string Status,
        string Priority,
        string SlaState,
        DateTime? SlaDueAt);

    private sealed record UpdateTicketStatusResponse(
        Guid TicketId,
        string TicketNumber,
        string PreviousStatus,
        string NewStatus,
        string Message);

    private sealed record UpdateTicketPriorityResponse(
        Guid TicketId,
        string TicketNumber,
        string PreviousPriority,
        string NewPriority,
        string Message);

    private sealed record TicketDetailDto(
        Guid Id,
        string TicketNumber,
        Guid CustomerId,
        string CustomerName,
        Guid AccountId,
        string AccountName,
        Guid CampaignId,
        string CampaignName,
        string Category,
        string Subject,
        string Description,
        string Priority,
        string Status,
        string SlaState,
        DateTime? SlaDueAt,
        bool IsEscalated,
        Guid CreatedByUserId,
        DateTime CreatedAt,
        DateTime? UpdatedAt,
        Guid? AssignedAgentUserId,
        string? AssignedAgentName);

    private sealed record SlaSnapshot(
        DateTime? TicketSlaDueAt,
        DateTime SlaStateDueAt,
        string SlaState,
        DateTime? LastEvaluatedAt);

    internal sealed class TicketStatusWorkflowApiFactory : WebApplicationFactory<Program>
    {
        public const string JwtSigningKey = "integration-testing-only-fictional-jwt-signing-key";

        private readonly SqliteConnection connection = new("Data Source=:memory:");

        public static async Task<TicketStatusWorkflowApiFactory> CreateAsync()
        {
            ConfigureEnvironment();
            var factory = new TicketStatusWorkflowApiFactory();
            await factory.connection.OpenAsync();
            await factory.InitializeDatabaseAsync();
            return factory;
        }

        protected override void ConfigureWebHost(IWebHostBuilder builder)
        {
            builder.UseEnvironment("Testing");
            builder.ConfigureAppConfiguration((_, config) =>
            {
                config.AddInMemoryCollection(new Dictionary<string, string?>
                {
                    ["ConnectionStrings:DefaultConnection"] = "Server=(local);Database=OpsSphereTicketStatusWorkflowTests;Trusted_Connection=True;TrustServerCertificate=True;",
                    ["SeedData:Enabled"] = "false",
                    ["Jwt:Issuer"] = "OpsSphere.Tests",
                    ["Jwt:Audience"] = "OpsSphere.Tests.Angular",
                    ["Jwt:ExpirationMinutes"] = "60",
                    ["Jwt:SigningKey"] = JwtSigningKey
                });
            });
            builder.ConfigureTestServices(services =>
            {
                services.RemoveAll<IDbContextOptionsConfiguration<OpsSphereDbContext>>();
                services.RemoveAll<DbContextOptions<OpsSphereDbContext>>();
                services.AddDbContext<OpsSphereDbContext>(options => options.UseSqlite(connection));
            });
        }

        private static void ConfigureEnvironment()
        {
            Environment.SetEnvironmentVariable("ConnectionStrings__DefaultConnection", "Server=(local);Database=OpsSphereTicketStatusWorkflowTests;Trusted_Connection=True;TrustServerCertificate=True;");
            Environment.SetEnvironmentVariable("SeedData__Enabled", "false");
            Environment.SetEnvironmentVariable("Jwt__Issuer", "OpsSphere.Tests");
            Environment.SetEnvironmentVariable("Jwt__Audience", "OpsSphere.Tests.Angular");
            Environment.SetEnvironmentVariable("Jwt__ExpirationMinutes", "60");
            Environment.SetEnvironmentVariable("Jwt__SigningKey", JwtSigningKey);
        }

        public override async ValueTask DisposeAsync()
        {
            await connection.DisposeAsync();
            await base.DisposeAsync();
        }

        private async Task InitializeDatabaseAsync()
        {
            using var scope = Services.CreateScope();
            var dbContext = scope.ServiceProvider.GetRequiredService<OpsSphereDbContext>();
            await dbContext.Database.EnsureDeletedAsync();
            await dbContext.Database.EnsureCreatedAsync();

            var seeder = scope.ServiceProvider.GetRequiredService<OpsSphereDataSeeder>();
            await seeder.SeedAsync();
        }
    }
}
