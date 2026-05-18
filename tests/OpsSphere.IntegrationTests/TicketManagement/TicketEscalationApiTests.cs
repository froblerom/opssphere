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

public sealed class TicketEscalationApiTests
{
    private const string AdminEmail = "admin@opssphere.local";
    private const string AgentEmail = "agent.novabank@opssphere.local";
    private const string SupervisorEmail = "supervisor.novabank@opssphere.local";
    private const string ViewerEmail = "viewer.latam@opssphere.local";
    private const string ManagerEmail = "manager.latam@opssphere.local";

    private static readonly JsonSerializerOptions JsonOptions = new(JsonSerializerDefaults.Web);

    [Fact]
    public async Task Supervisor_CanEscalate_OpenTicket_Returns200()
    {
        await using var factory = await TicketEscalationApiFactory.CreateAsync();
        var supervisor = await CreateAuthenticatedClientAsync(factory, SupervisorEmail);
        var ticket = await CreateNovaBankTicketAsync(supervisor, "Supervisor escalates open");

        var response = await EscalateAsync(supervisor, ticket.Id, "Customer impact requires supervisor attention");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var result = await ReadDataAsync<EscalateTicketResponse>(response);
        Assert.Equal(ticket.Id, result.TicketId);
        Assert.Equal(ticket.TicketNumber, result.TicketNumber);
        Assert.Equal("Open", result.PreviousStatus);
        Assert.Equal("Escalated", result.NewStatus);
        Assert.Equal("Ticket escalated successfully.", result.Message);
        Assert.NotEqual(Guid.Empty, result.EscalationId);
    }

    [Fact]
    public async Task Supervisor_CanEscalate_AssignedTicket_Returns200()
    {
        await using var factory = await TicketEscalationApiFactory.CreateAsync();
        var supervisor = await CreateAuthenticatedClientAsync(factory, SupervisorEmail);
        var ticket = await CreateNovaBankTicketAsync(supervisor, "Supervisor escalates assigned");
        await SetTicketStatusAsync(factory, ticket.Id, TicketStatus.Assigned);

        var response = await EscalateAsync(supervisor, ticket.Id, "Assigned ticket needs escalation");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var result = await ReadDataAsync<EscalateTicketResponse>(response);
        Assert.Equal("Assigned", result.PreviousStatus);
        Assert.Equal("Escalated", result.NewStatus);
    }

    [Fact]
    public async Task Supervisor_CanEscalate_InProgressTicket_Returns200()
    {
        await using var factory = await TicketEscalationApiFactory.CreateAsync();
        var supervisor = await CreateAuthenticatedClientAsync(factory, SupervisorEmail);
        var ticket = await CreateNovaBankTicketAsync(supervisor, "Supervisor escalates in progress");
        await SetTicketStatusAsync(factory, ticket.Id, TicketStatus.InProgress);

        var response = await EscalateAsync(supervisor, ticket.Id, "Investigation needs leadership visibility");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var result = await ReadDataAsync<EscalateTicketResponse>(response);
        Assert.Equal("InProgress", result.PreviousStatus);
    }

    [Fact]
    public async Task Supervisor_CanEscalate_WaitingForCustomerTicket_Returns200()
    {
        await using var factory = await TicketEscalationApiFactory.CreateAsync();
        var supervisor = await CreateAuthenticatedClientAsync(factory, SupervisorEmail);
        var ticket = await CreateNovaBankTicketAsync(supervisor, "Supervisor escalates waiting");
        await SetTicketStatusAsync(factory, ticket.Id, TicketStatus.WaitingForCustomer);

        var response = await EscalateAsync(supervisor, ticket.Id, "Customer-facing delay needs escalation");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var result = await ReadDataAsync<EscalateTicketResponse>(response);
        Assert.Equal("WaitingForCustomer", result.PreviousStatus);
    }

    [Fact]
    public async Task Agent_CanEscalate_WithinCampaignScope_Returns200()
    {
        await using var factory = await TicketEscalationApiFactory.CreateAsync();
        var agent = await CreateAuthenticatedClientAsync(factory, AgentEmail);
        var ticket = await CreateNovaBankTicketAsync(agent, "Agent escalates scoped ticket");

        var response = await EscalateAsync(agent, ticket.Id, "Agent needs supervisor support");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var result = await ReadDataAsync<EscalateTicketResponse>(response);
        Assert.Equal(ticket.Id, result.TicketId);
    }

    [Fact]
    public async Task Unauthenticated_CannotEscalate_Returns401()
    {
        await using var factory = await TicketEscalationApiFactory.CreateAsync();
        var supervisor = await CreateAuthenticatedClientAsync(factory, SupervisorEmail);
        var ticket = await CreateNovaBankTicketAsync(supervisor, "Anonymous escalation forbidden");
        var anonymous = factory.CreateClient();

        var response = await EscalateAsync(anonymous, ticket.Id, "Anonymous reason");

        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    [Fact]
    public async Task Admin_CannotEscalate_Returns403()
    {
        await using var factory = await TicketEscalationApiFactory.CreateAsync();
        var supervisor = await CreateAuthenticatedClientAsync(factory, SupervisorEmail);
        var admin = await CreateAuthenticatedClientAsync(factory, AdminEmail);
        var ticket = await CreateNovaBankTicketAsync(supervisor, "Admin escalation forbidden");

        var response = await EscalateAsync(admin, ticket.Id, "Admin reason");

        Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);
    }

    [Fact]
    public async Task OperationsManager_CannotEscalate_Returns403()
    {
        await using var factory = await TicketEscalationApiFactory.CreateAsync();
        var supervisor = await CreateAuthenticatedClientAsync(factory, SupervisorEmail);
        var manager = await CreateAuthenticatedClientAsync(factory, ManagerEmail);
        var ticket = await CreateNovaBankTicketAsync(supervisor, "Manager escalation forbidden");

        var response = await EscalateAsync(manager, ticket.Id, "Manager reason");

        Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);
    }

    [Fact]
    public async Task Viewer_CannotEscalate_Returns403()
    {
        await using var factory = await TicketEscalationApiFactory.CreateAsync();
        var supervisor = await CreateAuthenticatedClientAsync(factory, SupervisorEmail);
        var viewer = await CreateAuthenticatedClientAsync(factory, ViewerEmail);
        var ticket = await CreateNovaBankTicketAsync(supervisor, "Viewer escalation forbidden");

        var response = await EscalateAsync(viewer, ticket.Id, "Viewer reason");

        Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);
    }

    [Fact]
    public async Task Escalate_EmptyReason_Returns400_WithValidationError()
    {
        await using var factory = await TicketEscalationApiFactory.CreateAsync();
        var supervisor = await CreateAuthenticatedClientAsync(factory, SupervisorEmail);
        var ticket = await CreateNovaBankTicketAsync(supervisor, "Empty reason");

        var response = await EscalateAsync(supervisor, ticket.Id, string.Empty);

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        var error = await ReadErrorAsync(response);
        Assert.Equal("validation_error", error.Error.Code);
        Assert.Contains(error.Error.Details ?? [], d => d.Field == "escalationReason");
    }

    [Fact]
    public async Task Escalate_WhitespaceReason_Returns400_WithValidationError()
    {
        await using var factory = await TicketEscalationApiFactory.CreateAsync();
        var supervisor = await CreateAuthenticatedClientAsync(factory, SupervisorEmail);
        var ticket = await CreateNovaBankTicketAsync(supervisor, "Whitespace reason");

        var response = await EscalateAsync(supervisor, ticket.Id, "   ");

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        var error = await ReadErrorAsync(response);
        Assert.Equal("validation_error", error.Error.Code);
        Assert.Contains(error.Error.Details ?? [], d => d.Field == "escalationReason");
    }

    [Fact]
    public async Task Escalate_TooLongReason_Returns400_WithValidationError()
    {
        await using var factory = await TicketEscalationApiFactory.CreateAsync();
        var supervisor = await CreateAuthenticatedClientAsync(factory, SupervisorEmail);
        var ticket = await CreateNovaBankTicketAsync(supervisor, "Long reason");

        var response = await EscalateAsync(supervisor, ticket.Id, new string('a', 1001));

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        var error = await ReadErrorAsync(response);
        Assert.Equal("validation_error", error.Error.Code);
        Assert.Contains(error.Error.Details ?? [], d => d.Field == "escalationReason");
    }

    [Fact]
    public async Task Escalate_CrossScopeTicket_Returns404()
    {
        await using var factory = await TicketEscalationApiFactory.CreateAsync();
        var supervisor = await CreateAuthenticatedClientAsync(factory, SupervisorEmail);
        var outOfScopeTicketId = await AddTicketDirectlyAsync(
            factory,
            SeedIds.Accounts.Streamly,
            SeedIds.Campaigns.StreamlyCreator,
            SeedIds.Customers.StreamlyCustomer1,
            TicketStatus.Open,
            "Out-of-scope escalation");

        var response = await EscalateAsync(supervisor, outOfScopeTicketId, "Supervisor out-of-scope reason");

        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }

    [Fact]
    public async Task Escalate_AlreadyEscalatedTicket_Returns400()
    {
        await using var factory = await TicketEscalationApiFactory.CreateAsync();
        var supervisor = await CreateAuthenticatedClientAsync(factory, SupervisorEmail);
        var ticket = await CreateNovaBankTicketAsync(supervisor, "Already escalated");
        await AddEscalationDirectlyAsync(factory, ticket.Id, SeedIds.Users.SupervisorNovabank, "Existing escalation");

        var response = await EscalateAsync(supervisor, ticket.Id, "Duplicate escalation");

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        var error = await ReadErrorAsync(response);
        Assert.Equal("business_rule_violation", error.Error.Code);
        Assert.Contains("already", error.Error.Message, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public async Task Escalate_ClosedTicket_Returns400()
    {
        await using var factory = await TicketEscalationApiFactory.CreateAsync();
        var supervisor = await CreateAuthenticatedClientAsync(factory, SupervisorEmail);
        var ticket = await CreateNovaBankTicketAsync(supervisor, "Closed escalation");
        await SetTicketStatusAsync(factory, ticket.Id, TicketStatus.Closed);

        var response = await EscalateAsync(supervisor, ticket.Id, "Closed ticket reason");

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        var error = await ReadErrorAsync(response);
        Assert.Equal("business_rule_violation", error.Error.Code);
        Assert.Contains("closed", error.Error.Message, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public async Task Escalate_ResolvedTicket_Returns400()
    {
        await using var factory = await TicketEscalationApiFactory.CreateAsync();
        var supervisor = await CreateAuthenticatedClientAsync(factory, SupervisorEmail);
        var ticket = await CreateNovaBankTicketAsync(supervisor, "Resolved escalation");
        await SetTicketStatusAsync(factory, ticket.Id, TicketStatus.Resolved);

        var response = await EscalateAsync(supervisor, ticket.Id, "Resolved ticket reason");

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        var error = await ReadErrorAsync(response);
        Assert.Equal("business_rule_violation", error.Error.Code);
        Assert.Contains("resolved", error.Error.Message, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public async Task Escalate_UpdatesStatusToEscalated()
    {
        await using var factory = await TicketEscalationApiFactory.CreateAsync();
        var supervisor = await CreateAuthenticatedClientAsync(factory, SupervisorEmail);
        var ticket = await CreateNovaBankTicketAsync(supervisor, "Status persistence");

        await EscalateAsync(supervisor, ticket.Id, "Persist escalated status");

        var persisted = await GetTicketAsync(factory, ticket.Id);
        Assert.Equal(TicketStatus.Escalated, persisted.Status);
    }

    [Fact]
    public async Task Escalate_SetsIsEscalatedTrue()
    {
        await using var factory = await TicketEscalationApiFactory.CreateAsync();
        var supervisor = await CreateAuthenticatedClientAsync(factory, SupervisorEmail);
        var ticket = await CreateNovaBankTicketAsync(supervisor, "Escalated flag");

        await EscalateAsync(supervisor, ticket.Id, "Persist escalated flag");

        var persisted = await GetTicketAsync(factory, ticket.Id);
        Assert.True(persisted.IsEscalated);
    }

    [Fact]
    public async Task Escalate_CreatesTicketEscalationRecord()
    {
        await using var factory = await TicketEscalationApiFactory.CreateAsync();
        var supervisor = await CreateAuthenticatedClientAsync(factory, SupervisorEmail);
        var ticket = await CreateNovaBankTicketAsync(supervisor, "Escalation record");

        var response = await EscalateAsync(supervisor, ticket.Id, "Create escalation record");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var result = await ReadDataAsync<EscalateTicketResponse>(response);
        using var scope = factory.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<OpsSphereDbContext>();
        var escalation = await db.TicketEscalations.AsNoTracking().SingleAsync(e => e.Id == result.EscalationId);
        Assert.Equal(ticket.Id, escalation.TicketId);
        Assert.Equal(SeedIds.Users.SupervisorNovabank, escalation.EscalatedByUserId);
        Assert.True(escalation.IsActive);
        Assert.Null(escalation.ReviewedByUserId);
        Assert.Null(escalation.ReviewNotes);
        Assert.Null(escalation.ResolvedAt);
    }

    [Fact]
    public async Task Escalate_StoresTrimmedEscalationReason()
    {
        await using var factory = await TicketEscalationApiFactory.CreateAsync();
        var supervisor = await CreateAuthenticatedClientAsync(factory, SupervisorEmail);
        var ticket = await CreateNovaBankTicketAsync(supervisor, "Trim escalation reason");

        var response = await EscalateAsync(supervisor, ticket.Id, "  Trimmed escalation reason  ");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var result = await ReadDataAsync<EscalateTicketResponse>(response);
        using var scope = factory.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<OpsSphereDbContext>();
        var escalation = await db.TicketEscalations.AsNoTracking().SingleAsync(e => e.Id == result.EscalationId);
        Assert.Equal("Trimmed escalation reason", escalation.EscalationReason);
    }

    [Fact]
    public async Task Escalate_CreatesStatusHistoryRecord()
    {
        await using var factory = await TicketEscalationApiFactory.CreateAsync();
        var supervisor = await CreateAuthenticatedClientAsync(factory, SupervisorEmail);
        var ticket = await CreateNovaBankTicketAsync(supervisor, "Escalation status history");

        await EscalateAsync(supervisor, ticket.Id, "History reason");

        using var scope = factory.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<OpsSphereDbContext>();
        var history = await db.TicketStatusHistory
            .AsNoTracking()
            .SingleOrDefaultAsync(h => h.TicketId == ticket.Id && h.PreviousStatus == "Open" && h.NewStatus == "Escalated");
        Assert.NotNull(history);
        Assert.Equal(SeedIds.Users.SupervisorNovabank, history.ChangedByUserId);
        Assert.Equal("Ticket escalated", history.ChangeReason);
    }

    [Fact]
    public async Task Escalate_CreatesAuditLog_TicketEscalated()
    {
        await using var factory = await TicketEscalationApiFactory.CreateAsync();
        var supervisor = await CreateAuthenticatedClientAsync(factory, SupervisorEmail);
        var ticket = await CreateNovaBankTicketAsync(supervisor, "Escalation audit");

        await EscalateAsync(supervisor, ticket.Id, "Audit reason");

        using var scope = factory.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<OpsSphereDbContext>();
        var audit = await db.AuditLogs
            .AsNoTracking()
            .SingleOrDefaultAsync(log => log.Action == "TicketEscalated" && log.EntityType == "Ticket" && log.EntityId == ticket.Id);
        Assert.NotNull(audit);
        Assert.False(string.IsNullOrWhiteSpace(audit.CorrelationId));
        Assert.Contains("Open", $"{audit.PreviousValue}{audit.NewValue}", StringComparison.OrdinalIgnoreCase);
        Assert.Contains("Escalated", $"{audit.PreviousValue}{audit.NewValue}", StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public async Task Escalate_AuditLogDoesNotContainReasonOrPii()
    {
        await using var factory = await TicketEscalationApiFactory.CreateAsync();
        var supervisor = await CreateAuthenticatedClientAsync(factory, SupervisorEmail);
        const string sensitiveSubject = "EscalationSensitiveSubject-ABC123";
        const string sensitiveDescription = "EscalationSensitiveDescription-XYZ987";
        const string sensitiveReason = "EscalationSensitiveReason-SECRET456";
        var ticket = await CreateNovaBankTicketAsync(supervisor, sensitiveSubject, sensitiveDescription);

        await EscalateAsync(supervisor, ticket.Id, sensitiveReason);

        var combined = await GetAuditPayloadAsync(factory, ticket.Id);
        Assert.DoesNotContain(sensitiveReason, combined, StringComparison.OrdinalIgnoreCase);
        Assert.DoesNotContain(sensitiveSubject, combined, StringComparison.OrdinalIgnoreCase);
        Assert.DoesNotContain(sensitiveDescription, combined, StringComparison.OrdinalIgnoreCase);
        Assert.DoesNotContain("Carlos", combined, StringComparison.OrdinalIgnoreCase);
        Assert.DoesNotContain("Mendez", combined, StringComparison.OrdinalIgnoreCase);
        Assert.DoesNotContain("carlos.mendez", combined, StringComparison.OrdinalIgnoreCase);
        Assert.DoesNotContain("+52-55", combined, StringComparison.OrdinalIgnoreCase);
        Assert.DoesNotContain("Lina", combined, StringComparison.OrdinalIgnoreCase);
        Assert.DoesNotContain("Calderon", combined, StringComparison.OrdinalIgnoreCase);
        Assert.DoesNotContain(SupervisorEmail, combined, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public async Task Escalate_AuditLogPropertyShape_IsExpected()
    {
        await using var factory = await TicketEscalationApiFactory.CreateAsync();
        var supervisor = await CreateAuthenticatedClientAsync(factory, SupervisorEmail);
        var ticket = await CreateNovaBankTicketAsync(supervisor, "Escalation audit shape");

        await EscalateAsync(supervisor, ticket.Id, "Audit shape reason");

        using var scope = factory.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<OpsSphereDbContext>();
        var audit = await db.AuditLogs
            .AsNoTracking()
            .SingleAsync(log => log.Action == "TicketEscalated" && log.EntityId == ticket.Id);
        using var previous = JsonDocument.Parse(audit.PreviousValue ?? "{}");
        using var next = JsonDocument.Parse(audit.NewValue ?? "{}");

        Assert.Equal(["status"], previous.RootElement.EnumerateObject().Select(p => p.Name).ToArray());
        Assert.Equal(["escalatedByUserId", "status"], next.RootElement.EnumerateObject().Select(p => p.Name).ToArray());
        Assert.Equal("Open", previous.RootElement.GetProperty("status").GetString());
        Assert.Equal(SeedIds.Users.SupervisorNovabank, next.RootElement.GetProperty("escalatedByUserId").GetGuid());
        Assert.Equal("Escalated", next.RootElement.GetProperty("status").GetString());
    }

    [Fact]
    public async Task EscalationQueue_Supervisor_ReturnsScopedEscalations()
    {
        await using var factory = await TicketEscalationApiFactory.CreateAsync();
        var supervisor = await CreateAuthenticatedClientAsync(factory, SupervisorEmail);
        var inScopeTicketId = await AddTicketDirectlyAsync(factory, SeedIds.Accounts.NovaBank, SeedIds.Campaigns.NovaBankCreditCard, SeedIds.Customers.NovaBankCustomer1, TicketStatus.Open, "Supervisor queue scoped");
        var outOfScopeTicketId = await AddTicketDirectlyAsync(factory, SeedIds.Accounts.Shopora, SeedIds.Campaigns.ShoporaAccess, SeedIds.Customers.ShoporaCustomer1, TicketStatus.Open, "Supervisor queue out of scope");
        await AddEscalationDirectlyAsync(factory, inScopeTicketId, SeedIds.Users.SupervisorNovabank, "In-scope supervisor reason");
        await AddEscalationDirectlyAsync(factory, outOfScopeTicketId, SeedIds.Users.SupervisorNovabank, "Out-of-scope supervisor reason");

        var response = await supervisor.GetAsync("/api/tickets/escalations");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var queue = await ReadDataAsync<IReadOnlyList<EscalationQueueItemDto>>(response);
        Assert.Contains(queue, item => item.TicketId == inScopeTicketId);
        Assert.DoesNotContain(queue, item => item.TicketId == outOfScopeTicketId);
    }

    [Fact]
    public async Task EscalationQueue_OperationsManager_CanViewScopedEscalations()
    {
        await using var factory = await TicketEscalationApiFactory.CreateAsync();
        var manager = await CreateAuthenticatedClientAsync(factory, ManagerEmail);
        var ticketId = await AddTicketDirectlyAsync(factory, SeedIds.Accounts.NovaBank, SeedIds.Campaigns.NovaBankCreditCard, SeedIds.Customers.NovaBankCustomer1, TicketStatus.Open, "Manager queue scoped");
        await AddEscalationDirectlyAsync(factory, ticketId, SeedIds.Users.SupervisorNovabank, "Manager visible reason");

        var response = await manager.GetAsync("/api/tickets/escalations");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var queue = await ReadDataAsync<IReadOnlyList<EscalationQueueItemDto>>(response);
        Assert.Contains(queue, item => item.TicketId == ticketId);
    }

    [Fact]
    public async Task EscalationQueue_Viewer_CanViewScopedEscalations()
    {
        await using var factory = await TicketEscalationApiFactory.CreateAsync();
        var viewer = await CreateAuthenticatedClientAsync(factory, ViewerEmail);
        var ticketId = await AddTicketDirectlyAsync(factory, SeedIds.Accounts.NovaBank, SeedIds.Campaigns.NovaBankCreditCard, SeedIds.Customers.NovaBankCustomer1, TicketStatus.Open, "Viewer queue scoped");
        await AddEscalationDirectlyAsync(factory, ticketId, SeedIds.Users.SupervisorNovabank, "Viewer visible reason");

        var response = await viewer.GetAsync("/api/tickets/escalations");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var queue = await ReadDataAsync<IReadOnlyList<EscalationQueueItemDto>>(response);
        Assert.Contains(queue, item => item.TicketId == ticketId);
    }

    [Fact]
    public async Task EscalationQueue_ExcludesNonEscalatedTickets()
    {
        await using var factory = await TicketEscalationApiFactory.CreateAsync();
        var supervisor = await CreateAuthenticatedClientAsync(factory, SupervisorEmail);
        var ticketId = await AddTicketDirectlyAsync(factory, SeedIds.Accounts.NovaBank, SeedIds.Campaigns.NovaBankCreditCard, SeedIds.Customers.NovaBankCustomer1, TicketStatus.Open, "Queue non-escalated excluded");

        var response = await supervisor.GetAsync("/api/tickets/escalations");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var queue = await ReadDataAsync<IReadOnlyList<EscalationQueueItemDto>>(response);
        Assert.DoesNotContain(queue, item => item.TicketId == ticketId);
    }

    [Fact]
    public async Task EscalationQueue_ExcludesOutOfScopeEscalations()
    {
        await using var factory = await TicketEscalationApiFactory.CreateAsync();
        var supervisor = await CreateAuthenticatedClientAsync(factory, SupervisorEmail);
        var outOfScopeTicketId = await AddTicketDirectlyAsync(factory, SeedIds.Accounts.Streamly, SeedIds.Campaigns.StreamlyCreator, SeedIds.Customers.StreamlyCustomer1, TicketStatus.Open, "Queue out-of-scope excluded");
        await AddEscalationDirectlyAsync(factory, outOfScopeTicketId, SeedIds.Users.SupervisorNovabank, "Out-of-scope reason");

        var response = await supervisor.GetAsync("/api/tickets/escalations");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var queue = await ReadDataAsync<IReadOnlyList<EscalationQueueItemDto>>(response);
        Assert.DoesNotContain(queue, item => item.TicketId == outOfScopeTicketId);
    }

    [Fact]
    public async Task EscalationQueue_ReturnsActiveEscalationsOnly()
    {
        await using var factory = await TicketEscalationApiFactory.CreateAsync();
        var supervisor = await CreateAuthenticatedClientAsync(factory, SupervisorEmail);
        var activeTicketId = await AddTicketDirectlyAsync(factory, SeedIds.Accounts.NovaBank, SeedIds.Campaigns.NovaBankCreditCard, SeedIds.Customers.NovaBankCustomer1, TicketStatus.Open, "Active escalation");
        var inactiveTicketId = await AddTicketDirectlyAsync(factory, SeedIds.Accounts.NovaBank, SeedIds.Campaigns.NovaBankCreditCard, SeedIds.Customers.NovaBankCustomer1, TicketStatus.Open, "Inactive escalation");
        await AddEscalationDirectlyAsync(factory, activeTicketId, SeedIds.Users.SupervisorNovabank, "Active reason", isActive: true);
        await AddEscalationDirectlyAsync(factory, inactiveTicketId, SeedIds.Users.SupervisorNovabank, "Inactive reason", isActive: false);

        var response = await supervisor.GetAsync("/api/tickets/escalations");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var queue = await ReadDataAsync<IReadOnlyList<EscalationQueueItemDto>>(response);
        Assert.Contains(queue, item => item.TicketId == activeTicketId);
        Assert.DoesNotContain(queue, item => item.TicketId == inactiveTicketId);
    }

    [Fact]
    public async Task EscalationQueue_SortedNewestFirst()
    {
        await using var factory = await TicketEscalationApiFactory.CreateAsync();
        var supervisor = await CreateAuthenticatedClientAsync(factory, SupervisorEmail);
        var olderTicketId = await AddTicketDirectlyAsync(factory, SeedIds.Accounts.NovaBank, SeedIds.Campaigns.NovaBankCreditCard, SeedIds.Customers.NovaBankCustomer1, TicketStatus.Open, "Older escalation");
        var newerTicketId = await AddTicketDirectlyAsync(factory, SeedIds.Accounts.NovaBank, SeedIds.Campaigns.NovaBankCreditCard, SeedIds.Customers.NovaBankCustomer1, TicketStatus.Open, "Newer escalation");
        await AddEscalationDirectlyAsync(factory, olderTicketId, SeedIds.Users.SupervisorNovabank, "Older reason", DateTime.UtcNow.AddMinutes(-10));
        await AddEscalationDirectlyAsync(factory, newerTicketId, SeedIds.Users.SupervisorNovabank, "Newer reason", DateTime.UtcNow);

        var response = await supervisor.GetAsync("/api/tickets/escalations");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var queue = await ReadDataAsync<IReadOnlyList<EscalationQueueItemDto>>(response);
        Assert.True(queue.Count >= 2);
        Assert.Equal(newerTicketId, queue[0].TicketId);
        Assert.Equal(olderTicketId, queue[1].TicketId);
    }

    [Fact]
    public async Task EscalationQueue_IncludesEscalationReason()
    {
        await using var factory = await TicketEscalationApiFactory.CreateAsync();
        var supervisor = await CreateAuthenticatedClientAsync(factory, SupervisorEmail);
        const string reason = "Queue should show operational escalation reason";
        var ticketId = await AddTicketDirectlyAsync(factory, SeedIds.Accounts.NovaBank, SeedIds.Campaigns.NovaBankCreditCard, SeedIds.Customers.NovaBankCustomer1, TicketStatus.Open, "Queue reason included");
        await AddEscalationDirectlyAsync(factory, ticketId, SeedIds.Users.SupervisorNovabank, reason);

        var response = await supervisor.GetAsync("/api/tickets/escalations");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var queue = await ReadDataAsync<IReadOnlyList<EscalationQueueItemDto>>(response);
        var item = Assert.Single(queue, i => i.TicketId == ticketId);
        Assert.Equal(reason, item.EscalationReason);
        Assert.Equal("Lina Calderon", item.EscalatedByName);
        Assert.Equal("NovaBank", item.AccountName);
        Assert.Equal("Credit Card Support", item.CampaignName);
    }

    [Fact]
    public async Task Escalate_BusinessRuleError_UsesCanonicalEnvelope()
    {
        await using var factory = await TicketEscalationApiFactory.CreateAsync();
        var supervisor = await CreateAuthenticatedClientAsync(factory, SupervisorEmail);
        var ticket = await CreateNovaBankTicketAsync(supervisor, "Escalation canonical envelope");
        await AddEscalationDirectlyAsync(factory, ticket.Id, SeedIds.Users.SupervisorNovabank, "Existing escalation");

        var response = await EscalateAsync(supervisor, ticket.Id, "Duplicate reason");

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        var error = await ReadErrorAsync(response);
        Assert.Equal("business_rule_violation", error.Error.Code);
        Assert.False(string.IsNullOrWhiteSpace(error.Error.Message));
        Assert.False(string.IsNullOrWhiteSpace(error.Error.CorrelationId));
    }

    private static Task<HttpResponseMessage> EscalateAsync(
        HttpClient client,
        Guid ticketId,
        string? escalationReason)
    {
        return client.PostAsJsonAsync($"/api/tickets/{ticketId}/escalate", new
        {
            escalationReason
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
        TicketEscalationApiFactory factory,
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
            IsEscalated = status == TicketStatus.Escalated,
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

    private static async Task<Guid> AddEscalationDirectlyAsync(
        TicketEscalationApiFactory factory,
        Guid ticketId,
        Guid escalatedByUserId,
        string escalationReason,
        DateTime? createdAt = null,
        bool isActive = true)
    {
        using var scope = factory.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<OpsSphereDbContext>();
        var ticket = await db.Tickets.SingleAsync(t => t.Id == ticketId);
        var now = createdAt ?? DateTime.UtcNow;
        ticket.Status = TicketStatus.Escalated;
        ticket.IsEscalated = true;
        ticket.UpdatedAt = now;

        var escalationId = Guid.NewGuid();
        db.TicketEscalations.Add(new TicketEscalation
        {
            Id = escalationId,
            TicketId = ticketId,
            EscalatedByUserId = escalatedByUserId,
            EscalationReason = escalationReason,
            ReviewedByUserId = null,
            ReviewNotes = null,
            ResolvedAt = null,
            IsActive = isActive,
            CreatedAt = now
        });
        await db.SaveChangesAsync();
        return escalationId;
    }

    private static async Task SetTicketStatusAsync(TicketEscalationApiFactory factory, Guid ticketId, TicketStatus status)
    {
        using var scope = factory.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<OpsSphereDbContext>();
        var ticket = await db.Tickets.SingleAsync(t => t.Id == ticketId);
        ticket.Status = status;
        ticket.IsEscalated = status == TicketStatus.Escalated || ticket.IsEscalated;
        await db.SaveChangesAsync();
    }

    private static async Task<Ticket> GetTicketAsync(TicketEscalationApiFactory factory, Guid ticketId)
    {
        using var scope = factory.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<OpsSphereDbContext>();
        return await db.Tickets.AsNoTracking().SingleAsync(t => t.Id == ticketId);
    }

    private static async Task<string> GetAuditPayloadAsync(TicketEscalationApiFactory factory, Guid ticketId)
    {
        using var scope = factory.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<OpsSphereDbContext>();
        var audit = await db.AuditLogs
            .AsNoTracking()
            .SingleAsync(log => log.Action == "TicketEscalated" && log.EntityType == "Ticket" && log.EntityId == ticketId);

        return $"{audit.PreviousValue}{audit.NewValue}";
    }

    private static async Task<HttpClient> CreateAuthenticatedClientAsync(TicketEscalationApiFactory factory, string email)
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

    private sealed record EscalateTicketResponse(
        Guid TicketId,
        string TicketNumber,
        Guid EscalationId,
        string PreviousStatus,
        string NewStatus,
        string Message);

    private sealed record EscalationQueueItemDto(
        Guid EscalationId,
        Guid TicketId,
        string TicketNumber,
        string CustomerName,
        string AccountName,
        string CampaignName,
        string Priority,
        string Status,
        string SlaState,
        DateTime EscalatedAt,
        Guid EscalatedByUserId,
        string EscalatedByName,
        string EscalationReason);

    internal sealed class TicketEscalationApiFactory : WebApplicationFactory<Program>
    {
        public const string JwtSigningKey = "integration-testing-only-fictional-jwt-signing-key";

        private readonly SqliteConnection connection = new("Data Source=:memory:");

        public static async Task<TicketEscalationApiFactory> CreateAsync()
        {
            ConfigureEnvironment();
            var factory = new TicketEscalationApiFactory();
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
                    ["ConnectionStrings:DefaultConnection"] = "Server=(local);Database=OpsSphereTicketEscalationTests;Trusted_Connection=True;TrustServerCertificate=True;",
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
            Environment.SetEnvironmentVariable("ConnectionStrings__DefaultConnection", "Server=(local);Database=OpsSphereTicketEscalationTests;Trusted_Connection=True;TrustServerCertificate=True;");
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
