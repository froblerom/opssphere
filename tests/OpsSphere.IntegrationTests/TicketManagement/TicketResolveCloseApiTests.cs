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

public sealed class TicketResolveCloseApiTests
{
    private const string AdminEmail = "admin@opssphere.local";
    private const string AgentEmail = "agent.novabank@opssphere.local";
    private const string SupervisorEmail = "supervisor.novabank@opssphere.local";
    private const string ViewerEmail = "viewer.latam@opssphere.local";
    private const string ManagerEmail = "manager.latam@opssphere.local";

    private static readonly JsonSerializerOptions JsonOptions = new(JsonSerializerDefaults.Web);

    // -------------------------------------------------------------------------
    // Resolve — Authorization
    // -------------------------------------------------------------------------

    [Fact]
    public async Task Supervisor_CanResolve_OpenTicket_Returns200()
    {
        await using var factory = await TicketResolveCloseApiFactory.CreateAsync();
        var supervisor = await CreateAuthenticatedClientAsync(factory, SupervisorEmail);
        var ticket = await CreateNovaBankTicketAsync(supervisor, "Supervisor resolves open");

        var response = await ResolveAsync(supervisor, ticket.Id, "Issue confirmed and resolved.", null);

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var result = await ReadDataAsync<ResolveTicketResponse>(response);
        Assert.Equal(ticket.Id, result.TicketId);
        Assert.Equal(ticket.TicketNumber, result.TicketNumber);
        Assert.Equal("Open", result.PreviousStatus);
        Assert.Equal("Resolved", result.NewStatus);
        Assert.Equal("Ticket resolved successfully.", result.Message);
        Assert.NotEqual(Guid.Empty, result.ResolutionId);
        Assert.False(string.IsNullOrWhiteSpace(result.FinalSlaState));
    }

    [Fact]
    public async Task Agent_CanResolve_WithinScope_Returns200()
    {
        await using var factory = await TicketResolveCloseApiFactory.CreateAsync();
        var agent = await CreateAuthenticatedClientAsync(factory, AgentEmail);
        var ticket = await CreateNovaBankTicketAsync(agent, "Agent resolves ticket");

        var response = await ResolveAsync(agent, ticket.Id, "Agent resolution summary.", null);

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var result = await ReadDataAsync<ResolveTicketResponse>(response);
        Assert.Equal("Resolved", result.NewStatus);
    }

    [Fact]
    public async Task OperationsManager_CanResolve_WithinScope_Returns200()
    {
        await using var factory = await TicketResolveCloseApiFactory.CreateAsync();
        var supervisor = await CreateAuthenticatedClientAsync(factory, SupervisorEmail);
        var manager = await CreateAuthenticatedClientAsync(factory, ManagerEmail);
        var ticket = await CreateNovaBankTicketAsync(supervisor, "Manager resolves ticket");

        var response = await ResolveAsync(manager, ticket.Id, "Manager resolution summary.", null);

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
    }

    [Fact]
    public async Task Admin_CannotResolve_Returns403()
    {
        await using var factory = await TicketResolveCloseApiFactory.CreateAsync();
        var supervisor = await CreateAuthenticatedClientAsync(factory, SupervisorEmail);
        var admin = await CreateAuthenticatedClientAsync(factory, AdminEmail);
        var ticket = await CreateNovaBankTicketAsync(supervisor, "Admin resolve forbidden");

        var response = await ResolveAsync(admin, ticket.Id, "Admin resolution.", null);

        Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);
    }

    [Fact]
    public async Task Viewer_CannotResolve_Returns403()
    {
        await using var factory = await TicketResolveCloseApiFactory.CreateAsync();
        var supervisor = await CreateAuthenticatedClientAsync(factory, SupervisorEmail);
        var viewer = await CreateAuthenticatedClientAsync(factory, ViewerEmail);
        var ticket = await CreateNovaBankTicketAsync(supervisor, "Viewer resolve forbidden");

        var response = await ResolveAsync(viewer, ticket.Id, "Viewer resolution.", null);

        Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);
    }

    [Fact]
    public async Task Unauthenticated_CannotResolve_Returns401()
    {
        await using var factory = await TicketResolveCloseApiFactory.CreateAsync();
        var supervisor = await CreateAuthenticatedClientAsync(factory, SupervisorEmail);
        var ticket = await CreateNovaBankTicketAsync(supervisor, "Unauthenticated resolve forbidden");
        var anonymous = factory.CreateClient();

        var response = await ResolveAsync(anonymous, ticket.Id, "Anonymous resolution.", null);

        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    // -------------------------------------------------------------------------
    // Resolve — Validation
    // -------------------------------------------------------------------------

    [Fact]
    public async Task Resolve_EmptySummary_Returns400_ValidationError()
    {
        await using var factory = await TicketResolveCloseApiFactory.CreateAsync();
        var agent = await CreateAuthenticatedClientAsync(factory, AgentEmail);
        var ticket = await CreateNovaBankTicketAsync(agent, "Empty resolution summary");

        var response = await ResolveAsync(agent, ticket.Id, string.Empty, null);

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        var error = await ReadErrorAsync(response);
        Assert.Equal("validation_error", error.Error.Code);
        Assert.Contains(error.Error.Details ?? [], d => d.Field == "resolutionSummary");
    }

    [Fact]
    public async Task Resolve_NullSummary_Returns400_ValidationError()
    {
        await using var factory = await TicketResolveCloseApiFactory.CreateAsync();
        var agent = await CreateAuthenticatedClientAsync(factory, AgentEmail);
        var ticket = await CreateNovaBankTicketAsync(agent, "Null resolution summary");

        var response = await ResolveAsync(agent, ticket.Id, null, null);

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        var error = await ReadErrorAsync(response);
        Assert.Equal("validation_error", error.Error.Code);
        Assert.Contains(error.Error.Details ?? [], d => d.Field == "resolutionSummary");
    }

    [Fact]
    public async Task Resolve_TooLongSummary_Returns400_ValidationError()
    {
        await using var factory = await TicketResolveCloseApiFactory.CreateAsync();
        var agent = await CreateAuthenticatedClientAsync(factory, AgentEmail);
        var ticket = await CreateNovaBankTicketAsync(agent, "Too long resolution summary");

        var response = await ResolveAsync(agent, ticket.Id, new string('a', 2001), null);

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        var error = await ReadErrorAsync(response);
        Assert.Equal("validation_error", error.Error.Code);
        Assert.Contains(error.Error.Details ?? [], d => d.Field == "resolutionSummary");
    }

    [Fact]
    public async Task Resolve_TooLongResolutionCode_Returns400_ValidationError()
    {
        await using var factory = await TicketResolveCloseApiFactory.CreateAsync();
        var agent = await CreateAuthenticatedClientAsync(factory, AgentEmail);
        var ticket = await CreateNovaBankTicketAsync(agent, "Too long resolution code");

        var response = await ResolveAsync(agent, ticket.Id, "Valid summary.", new string('x', 101));

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        var error = await ReadErrorAsync(response);
        Assert.Equal("validation_error", error.Error.Code);
        Assert.Contains(error.Error.Details ?? [], d => d.Field == "resolutionCode");
    }

    [Fact]
    public async Task Resolve_CrossScope_Returns404()
    {
        await using var factory = await TicketResolveCloseApiFactory.CreateAsync();
        var supervisor = await CreateAuthenticatedClientAsync(factory, SupervisorEmail);
        var outOfScopeTicketId = await AddTicketDirectlyAsync(factory, SeedIds.Accounts.Streamly, SeedIds.Campaigns.StreamlyCreator, SeedIds.Customers.StreamlyCustomer1, TicketStatus.Open, "Cross-scope resolve");

        var response = await ResolveAsync(supervisor, outOfScopeTicketId, "Cross scope summary.", null);

        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }

    // -------------------------------------------------------------------------
    // Resolve — Business rules
    // -------------------------------------------------------------------------

    [Fact]
    public async Task Resolve_AlreadyResolved_Returns400_BusinessRuleViolation()
    {
        await using var factory = await TicketResolveCloseApiFactory.CreateAsync();
        var agent = await CreateAuthenticatedClientAsync(factory, AgentEmail);
        var ticket = await CreateNovaBankTicketAsync(agent, "Already resolved rejection");
        await AddResolutionDirectlyAsync(factory, ticket.Id, SeedIds.Users.AgentNovabank, "First resolution", "WithinSla");

        var response = await ResolveAsync(agent, ticket.Id, "Second resolution attempt.", null);

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        var error = await ReadErrorAsync(response);
        Assert.Equal("business_rule_violation", error.Error.Code);
        Assert.Contains("already resolved", error.Error.Message, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public async Task Resolve_ClosedTicket_Returns400_BusinessRuleViolation()
    {
        await using var factory = await TicketResolveCloseApiFactory.CreateAsync();
        var agent = await CreateAuthenticatedClientAsync(factory, AgentEmail);
        var ticket = await CreateNovaBankTicketAsync(agent, "Closed ticket resolve rejection");
        await SetTicketStatusAsync(factory, ticket.Id, TicketStatus.Closed);

        var response = await ResolveAsync(agent, ticket.Id, "Closed ticket summary.", null);

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        var error = await ReadErrorAsync(response);
        Assert.Equal("business_rule_violation", error.Error.Code);
    }

    [Fact]
    public async Task Resolve_EscalatedTicket_DeactivatesEscalation_And_ClearsIsEscalated()
    {
        await using var factory = await TicketResolveCloseApiFactory.CreateAsync();
        var supervisor = await CreateAuthenticatedClientAsync(factory, SupervisorEmail);
        var ticket = await CreateNovaBankTicketAsync(supervisor, "Escalated then resolved");
        await AddEscalationDirectlyAsync(factory, ticket.Id, SeedIds.Users.SupervisorNovabank, "Needs resolution");

        var response = await ResolveAsync(supervisor, ticket.Id, "Issue escalated but resolved.", null);

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        using var scope = factory.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<OpsSphereDbContext>();
        var dbTicket = await db.Tickets.AsNoTracking().SingleAsync(t => t.Id == ticket.Id);
        Assert.False(dbTicket.IsEscalated);
        var activeEscalations = await db.TicketEscalations.AsNoTracking()
            .Where(e => e.TicketId == ticket.Id && e.IsActive)
            .ToListAsync();
        Assert.Empty(activeEscalations);
    }

    // -------------------------------------------------------------------------
    // Resolve — Persistence
    // -------------------------------------------------------------------------

    [Fact]
    public async Task Resolve_PersistsResolutionRecord()
    {
        await using var factory = await TicketResolveCloseApiFactory.CreateAsync();
        var agent = await CreateAuthenticatedClientAsync(factory, AgentEmail);
        var ticket = await CreateNovaBankTicketAsync(agent, "Resolution persistence check");

        var response = await ResolveAsync(agent, ticket.Id, "Customer issue confirmed resolved.", "RC-001");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var result = await ReadDataAsync<ResolveTicketResponse>(response);

        using var scope = factory.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<OpsSphereDbContext>();
        var resolution = await db.TicketResolutions.AsNoTracking().SingleOrDefaultAsync(r => r.Id == result.ResolutionId);
        Assert.NotNull(resolution);
        Assert.Equal(ticket.Id, resolution.TicketId);
        Assert.Equal(SeedIds.Users.AgentNovabank, resolution.ResolvedByUserId);
        Assert.Equal("Customer issue confirmed resolved.", resolution.ResolutionSummary);
        Assert.Equal("RC-001", resolution.ResolutionCode);
    }

    [Fact]
    public async Task Resolve_SetsTicketStatusToResolved_And_ResolvedAt()
    {
        await using var factory = await TicketResolveCloseApiFactory.CreateAsync();
        var agent = await CreateAuthenticatedClientAsync(factory, AgentEmail);
        var ticket = await CreateNovaBankTicketAsync(agent, "Ticket status resolved persistence");
        var before = DateTime.UtcNow;

        var response = await ResolveAsync(agent, ticket.Id, "Status resolved check.", null);

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        using var scope = factory.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<OpsSphereDbContext>();
        var dbTicket = await db.Tickets.AsNoTracking().SingleAsync(t => t.Id == ticket.Id);
        Assert.Equal(TicketStatus.Resolved, dbTicket.Status);
        Assert.NotNull(dbTicket.ResolvedAt);
        Assert.True(dbTicket.ResolvedAt >= before.AddSeconds(-1));
    }

    [Fact]
    public async Task Resolve_SetsSlaStateToCompleted()
    {
        await using var factory = await TicketResolveCloseApiFactory.CreateAsync();
        var agent = await CreateAuthenticatedClientAsync(factory, AgentEmail);
        var ticket = await CreateNovaBankTicketAsync(agent, "SLA state Completed on resolve");

        var response = await ResolveAsync(agent, ticket.Id, "SLA state check.", null);

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        using var scope = factory.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<OpsSphereDbContext>();
        var dbTicket = await db.Tickets.AsNoTracking().SingleAsync(t => t.Id == ticket.Id);
        Assert.Equal(SlaState.Completed, dbTicket.SlaState);

        var slaState = await db.TicketSlaStates.AsNoTracking().SingleOrDefaultAsync(s => s.TicketId == ticket.Id);
        Assert.NotNull(slaState);
        Assert.Equal(SlaState.Completed.ToString(), slaState.State);
        Assert.NotNull(slaState.CompletedAt);
        Assert.False(string.IsNullOrWhiteSpace(slaState.FinalState));
    }

    [Fact]
    public async Task Resolve_PreservesOriginalSlaStateAsFinalState()
    {
        await using var factory = await TicketResolveCloseApiFactory.CreateAsync();
        var agent = await CreateAuthenticatedClientAsync(factory, AgentEmail);
        var ticket = await CreateNovaBankTicketAsync(agent, "FinalSlaState preservation");
        await SetTicketSlaStateAsync(factory, ticket.Id, SlaState.Breached);

        var response = await ResolveAsync(agent, ticket.Id, "Breached SLA, still resolved.", null);

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var result = await ReadDataAsync<ResolveTicketResponse>(response);
        Assert.Equal("Breached", result.FinalSlaState);

        using var scope = factory.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<OpsSphereDbContext>();
        var slaRecord = await db.TicketSlaStates.AsNoTracking().SingleOrDefaultAsync(s => s.TicketId == ticket.Id);
        Assert.NotNull(slaRecord);
        Assert.Equal("Breached", slaRecord.FinalState);

        var resolution = await db.TicketResolutions.AsNoTracking().SingleOrDefaultAsync(r => r.TicketId == ticket.Id);
        Assert.NotNull(resolution);
        Assert.Equal("Breached", resolution.FinalSlaState);
    }

    [Fact]
    public async Task Resolve_CreatesStatusHistoryEntry()
    {
        await using var factory = await TicketResolveCloseApiFactory.CreateAsync();
        var agent = await CreateAuthenticatedClientAsync(factory, AgentEmail);
        var ticket = await CreateNovaBankTicketAsync(agent, "Resolve status history");

        var response = await ResolveAsync(agent, ticket.Id, "Status history resolve check.", null);

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        using var scope = factory.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<OpsSphereDbContext>();
        var history = await db.TicketStatusHistory.AsNoTracking()
            .SingleOrDefaultAsync(h => h.TicketId == ticket.Id && h.NewStatus == "Resolved");
        Assert.NotNull(history);
        Assert.Equal(SeedIds.Users.AgentNovabank, history.ChangedByUserId);
        Assert.Equal("Ticket resolved", history.ChangeReason);
    }

    [Fact]
    public async Task Resolve_CreatesAuditLog_TicketResolved()
    {
        await using var factory = await TicketResolveCloseApiFactory.CreateAsync();
        var agent = await CreateAuthenticatedClientAsync(factory, AgentEmail);
        var ticket = await CreateNovaBankTicketAsync(agent, "Resolve audit action");

        var response = await ResolveAsync(agent, ticket.Id, "Audit resolve check.", null);

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        using var scope = factory.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<OpsSphereDbContext>();
        var audit = await db.AuditLogs.AsNoTracking()
            .SingleOrDefaultAsync(log => log.Action == "TicketResolved" && log.EntityType == "Ticket" && log.EntityId == ticket.Id);
        Assert.NotNull(audit);
        Assert.False(string.IsNullOrWhiteSpace(audit.CorrelationId));
    }

    [Fact]
    public async Task Resolve_AuditLog_DoesNotContainResolutionSummaryOrPii()
    {
        await using var factory = await TicketResolveCloseApiFactory.CreateAsync();
        var agent = await CreateAuthenticatedClientAsync(factory, AgentEmail);
        const string sensitiveSubject = "ResolveSensitiveSubject-ABC123";
        const string sensitiveDescription = "ResolveSensitiveDescription-XYZ987";
        const string sensitiveSummary = "ResolveSensitiveSummary-SECRET456";
        var ticket = await CreateNovaBankTicketAsync(agent, sensitiveSubject, sensitiveDescription);

        var response = await ResolveAsync(agent, ticket.Id, sensitiveSummary, null);

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        using var scope = factory.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<OpsSphereDbContext>();
        var audit = await db.AuditLogs.AsNoTracking()
            .SingleAsync(log => log.Action == "TicketResolved" && log.EntityId == ticket.Id);
        var combined = $"{audit.PreviousValue}{audit.NewValue}";

        Assert.DoesNotContain(sensitiveSummary, combined, StringComparison.OrdinalIgnoreCase);
        Assert.DoesNotContain(sensitiveSubject, combined, StringComparison.OrdinalIgnoreCase);
        Assert.DoesNotContain(sensitiveDescription, combined, StringComparison.OrdinalIgnoreCase);
        Assert.DoesNotContain("Carlos", combined, StringComparison.OrdinalIgnoreCase);
        Assert.DoesNotContain("Mendez", combined, StringComparison.OrdinalIgnoreCase);
        Assert.DoesNotContain("Diego", combined, StringComparison.OrdinalIgnoreCase);
        Assert.DoesNotContain("Santos", combined, StringComparison.OrdinalIgnoreCase);
        Assert.DoesNotContain(AgentEmail, combined, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public async Task Resolve_AuditLog_PropertyShape_IsExpected()
    {
        await using var factory = await TicketResolveCloseApiFactory.CreateAsync();
        var agent = await CreateAuthenticatedClientAsync(factory, AgentEmail);
        var ticket = await CreateNovaBankTicketAsync(agent, "Resolve audit shape");

        var response = await ResolveAsync(agent, ticket.Id, "Audit shape check.", "SHAPE-01");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        using var scope = factory.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<OpsSphereDbContext>();
        var audit = await db.AuditLogs.AsNoTracking()
            .SingleAsync(log => log.Action == "TicketResolved" && log.EntityId == ticket.Id);

        using var previous = JsonDocument.Parse(audit.PreviousValue ?? "{}");
        using var next = JsonDocument.Parse(audit.NewValue ?? "{}");

        var prevProps = previous.RootElement.EnumerateObject().Select(p => p.Name).Order().ToArray();
        var nextProps = next.RootElement.EnumerateObject().Select(p => p.Name).Order().ToArray();

        Assert.Equal(["previousStatus", "slaState"], prevProps);
        Assert.Equal(["finalSlaState", "resolutionId", "resolvedByUserId", "status"], nextProps);
        Assert.Equal("Resolved", next.RootElement.GetProperty("status").GetString());
        Assert.Equal(SeedIds.Users.AgentNovabank, next.RootElement.GetProperty("resolvedByUserId").GetGuid());
    }

    [Fact]
    public async Task Resolve_WithOptionalCode_PersistsCode()
    {
        await using var factory = await TicketResolveCloseApiFactory.CreateAsync();
        var agent = await CreateAuthenticatedClientAsync(factory, AgentEmail);
        var ticket = await CreateNovaBankTicketAsync(agent, "Resolve with optional code");

        var response = await ResolveAsync(agent, ticket.Id, "Resolution with code.", "RESOLVED-001");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        using var scope = factory.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<OpsSphereDbContext>();
        var resolution = await db.TicketResolutions.AsNoTracking().SingleOrDefaultAsync(r => r.TicketId == ticket.Id);
        Assert.NotNull(resolution);
        Assert.Equal("RESOLVED-001", resolution.ResolutionCode);
    }

    [Fact]
    public async Task Resolve_WithoutCode_PersistsNullCode()
    {
        await using var factory = await TicketResolveCloseApiFactory.CreateAsync();
        var agent = await CreateAuthenticatedClientAsync(factory, AgentEmail);
        var ticket = await CreateNovaBankTicketAsync(agent, "Resolve without code");

        var response = await ResolveAsync(agent, ticket.Id, "Resolution without code.", null);

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        using var scope = factory.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<OpsSphereDbContext>();
        var resolution = await db.TicketResolutions.AsNoTracking().SingleOrDefaultAsync(r => r.TicketId == ticket.Id);
        Assert.NotNull(resolution);
        Assert.Null(resolution.ResolutionCode);
    }

    // -------------------------------------------------------------------------
    // Close — Authorization
    // -------------------------------------------------------------------------

    [Fact]
    public async Task Supervisor_CanClose_ResolvedTicket_Returns200()
    {
        await using var factory = await TicketResolveCloseApiFactory.CreateAsync();
        var supervisor = await CreateAuthenticatedClientAsync(factory, SupervisorEmail);
        var ticket = await CreateNovaBankTicketAsync(supervisor, "Supervisor closes resolved");
        await AddResolutionDirectlyAsync(factory, ticket.Id, SeedIds.Users.SupervisorNovabank, "Pre-resolved for close", "WithinSla");

        var response = await CloseAsync(supervisor, ticket.Id);

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var result = await ReadDataAsync<CloseTicketResponse>(response);
        Assert.Equal(ticket.Id, result.TicketId);
        Assert.Equal("Resolved", result.PreviousStatus);
        Assert.Equal("Closed", result.NewStatus);
        Assert.Equal("Ticket closed successfully.", result.Message);
    }

    [Fact]
    public async Task Agent_CanClose_ResolvedTicket_Returns200()
    {
        await using var factory = await TicketResolveCloseApiFactory.CreateAsync();
        var agent = await CreateAuthenticatedClientAsync(factory, AgentEmail);
        var ticket = await CreateNovaBankTicketAsync(agent, "Agent closes resolved");
        await AddResolutionDirectlyAsync(factory, ticket.Id, SeedIds.Users.AgentNovabank, "Agent pre-resolved", "WithinSla");

        var response = await CloseAsync(agent, ticket.Id);

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
    }

    [Fact]
    public async Task Admin_CannotClose_Returns403()
    {
        await using var factory = await TicketResolveCloseApiFactory.CreateAsync();
        var supervisor = await CreateAuthenticatedClientAsync(factory, SupervisorEmail);
        var admin = await CreateAuthenticatedClientAsync(factory, AdminEmail);
        var ticket = await CreateNovaBankTicketAsync(supervisor, "Admin close forbidden");
        await AddResolutionDirectlyAsync(factory, ticket.Id, SeedIds.Users.SupervisorNovabank, "Admin close forbidden resolution", "WithinSla");

        var response = await CloseAsync(admin, ticket.Id);

        Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);
    }

    [Fact]
    public async Task Viewer_CannotClose_Returns403()
    {
        await using var factory = await TicketResolveCloseApiFactory.CreateAsync();
        var supervisor = await CreateAuthenticatedClientAsync(factory, SupervisorEmail);
        var viewer = await CreateAuthenticatedClientAsync(factory, ViewerEmail);
        var ticket = await CreateNovaBankTicketAsync(supervisor, "Viewer close forbidden");
        await AddResolutionDirectlyAsync(factory, ticket.Id, SeedIds.Users.SupervisorNovabank, "Viewer close forbidden resolution", "WithinSla");

        var response = await CloseAsync(viewer, ticket.Id);

        Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);
    }

    [Fact]
    public async Task Unauthenticated_CannotClose_Returns401()
    {
        await using var factory = await TicketResolveCloseApiFactory.CreateAsync();
        var supervisor = await CreateAuthenticatedClientAsync(factory, SupervisorEmail);
        var ticket = await CreateNovaBankTicketAsync(supervisor, "Unauthenticated close forbidden");
        await AddResolutionDirectlyAsync(factory, ticket.Id, SeedIds.Users.SupervisorNovabank, "Unauthenticated resolution", "WithinSla");
        var anonymous = factory.CreateClient();

        var response = await CloseAsync(anonymous, ticket.Id);

        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    // -------------------------------------------------------------------------
    // Close — Business rules
    // -------------------------------------------------------------------------

    [Fact]
    public async Task Close_OpenTicket_Returns400_BusinessRuleViolation()
    {
        await using var factory = await TicketResolveCloseApiFactory.CreateAsync();
        var agent = await CreateAuthenticatedClientAsync(factory, AgentEmail);
        var ticket = await CreateNovaBankTicketAsync(agent, "Close open ticket rejection");

        var response = await CloseAsync(agent, ticket.Id);

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        var error = await ReadErrorAsync(response);
        Assert.Equal("business_rule_violation", error.Error.Code);
        Assert.Contains("resolved", error.Error.Message, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public async Task Close_InProgressTicket_Returns400_BusinessRuleViolation()
    {
        await using var factory = await TicketResolveCloseApiFactory.CreateAsync();
        var agent = await CreateAuthenticatedClientAsync(factory, AgentEmail);
        var ticket = await CreateNovaBankTicketAsync(agent, "Close in-progress rejection");
        await SetTicketStatusAsync(factory, ticket.Id, TicketStatus.InProgress);

        var response = await CloseAsync(agent, ticket.Id);

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        var error = await ReadErrorAsync(response);
        Assert.Equal("business_rule_violation", error.Error.Code);
    }

    [Fact]
    public async Task Close_EscalatedTicket_Returns400_BusinessRuleViolation()
    {
        await using var factory = await TicketResolveCloseApiFactory.CreateAsync();
        var supervisor = await CreateAuthenticatedClientAsync(factory, SupervisorEmail);
        var ticket = await CreateNovaBankTicketAsync(supervisor, "Close escalated rejection");
        await AddEscalationDirectlyAsync(factory, ticket.Id, SeedIds.Users.SupervisorNovabank, "Escalated ticket close rejection");

        var response = await CloseAsync(supervisor, ticket.Id);

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        var error = await ReadErrorAsync(response);
        Assert.Equal("business_rule_violation", error.Error.Code);
    }

    [Fact]
    public async Task Close_CrossScope_Returns404()
    {
        await using var factory = await TicketResolveCloseApiFactory.CreateAsync();
        var supervisor = await CreateAuthenticatedClientAsync(factory, SupervisorEmail);
        var outOfScopeTicketId = await AddTicketDirectlyAsync(factory, SeedIds.Accounts.Streamly, SeedIds.Campaigns.StreamlyCreator, SeedIds.Customers.StreamlyCustomer1, TicketStatus.Resolved, "Cross-scope close");

        var response = await CloseAsync(supervisor, outOfScopeTicketId);

        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }

    // -------------------------------------------------------------------------
    // Close — Persistence
    // -------------------------------------------------------------------------

    [Fact]
    public async Task Close_SetsTicketStatusToClosed_And_ClosedAt()
    {
        await using var factory = await TicketResolveCloseApiFactory.CreateAsync();
        var agent = await CreateAuthenticatedClientAsync(factory, AgentEmail);
        var ticket = await CreateNovaBankTicketAsync(agent, "Closed persistence check");
        await AddResolutionDirectlyAsync(factory, ticket.Id, SeedIds.Users.AgentNovabank, "Pre-resolved", "WithinSla");
        var before = DateTime.UtcNow;

        var response = await CloseAsync(agent, ticket.Id);

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        using var scope = factory.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<OpsSphereDbContext>();
        var dbTicket = await db.Tickets.AsNoTracking().SingleAsync(t => t.Id == ticket.Id);
        Assert.Equal(TicketStatus.Closed, dbTicket.Status);
        Assert.NotNull(dbTicket.ClosedAt);
        Assert.True(dbTicket.ClosedAt >= before.AddSeconds(-1));
    }

    [Fact]
    public async Task Close_CreatesStatusHistoryEntry()
    {
        await using var factory = await TicketResolveCloseApiFactory.CreateAsync();
        var agent = await CreateAuthenticatedClientAsync(factory, AgentEmail);
        var ticket = await CreateNovaBankTicketAsync(agent, "Close status history");
        await AddResolutionDirectlyAsync(factory, ticket.Id, SeedIds.Users.AgentNovabank, "Pre-resolved for history", "WithinSla");

        var response = await CloseAsync(agent, ticket.Id);

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        using var scope = factory.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<OpsSphereDbContext>();
        var history = await db.TicketStatusHistory.AsNoTracking()
            .SingleOrDefaultAsync(h => h.TicketId == ticket.Id && h.NewStatus == "Closed");
        Assert.NotNull(history);
        Assert.Equal(SeedIds.Users.AgentNovabank, history.ChangedByUserId);
        Assert.Equal("Ticket closed", history.ChangeReason);
    }

    [Fact]
    public async Task Close_CreatesAuditLog_TicketClosed()
    {
        await using var factory = await TicketResolveCloseApiFactory.CreateAsync();
        var agent = await CreateAuthenticatedClientAsync(factory, AgentEmail);
        var ticket = await CreateNovaBankTicketAsync(agent, "Close audit action");
        await AddResolutionDirectlyAsync(factory, ticket.Id, SeedIds.Users.AgentNovabank, "Pre-resolved for audit", "WithinSla");

        var response = await CloseAsync(agent, ticket.Id);

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        using var scope = factory.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<OpsSphereDbContext>();
        var audit = await db.AuditLogs.AsNoTracking()
            .SingleOrDefaultAsync(log => log.Action == "TicketClosed" && log.EntityType == "Ticket" && log.EntityId == ticket.Id);
        Assert.NotNull(audit);
        Assert.False(string.IsNullOrWhiteSpace(audit.CorrelationId));
    }

    [Fact]
    public async Task Close_AuditLog_PropertyShape_IsExpected()
    {
        await using var factory = await TicketResolveCloseApiFactory.CreateAsync();
        var agent = await CreateAuthenticatedClientAsync(factory, AgentEmail);
        var ticket = await CreateNovaBankTicketAsync(agent, "Close audit shape");
        await AddResolutionDirectlyAsync(factory, ticket.Id, SeedIds.Users.AgentNovabank, "Pre-resolved for shape", "WithinSla");

        var response = await CloseAsync(agent, ticket.Id);

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        using var scope = factory.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<OpsSphereDbContext>();
        var audit = await db.AuditLogs.AsNoTracking()
            .SingleAsync(log => log.Action == "TicketClosed" && log.EntityId == ticket.Id);

        using var previous = JsonDocument.Parse(audit.PreviousValue ?? "{}");
        using var next = JsonDocument.Parse(audit.NewValue ?? "{}");

        var prevProps = previous.RootElement.EnumerateObject().Select(p => p.Name).Order().ToArray();
        var nextProps = next.RootElement.EnumerateObject().Select(p => p.Name).Order().ToArray();

        Assert.Equal(["status"], prevProps);
        Assert.Equal(["closedByUserId", "status"], nextProps);
        Assert.Equal("Resolved", previous.RootElement.GetProperty("status").GetString());
        Assert.Equal("Closed", next.RootElement.GetProperty("status").GetString());
        Assert.Equal(SeedIds.Users.AgentNovabank, next.RootElement.GetProperty("closedByUserId").GetGuid());
    }

    [Fact]
    public async Task Close_DoesNotModifyResolutionRecord()
    {
        await using var factory = await TicketResolveCloseApiFactory.CreateAsync();
        var agent = await CreateAuthenticatedClientAsync(factory, AgentEmail);
        var ticket = await CreateNovaBankTicketAsync(agent, "Close does not modify resolution");
        await AddResolutionDirectlyAsync(factory, ticket.Id, SeedIds.Users.AgentNovabank, "Resolution before close", "WithinSla");

        using var scopeBefore = factory.Services.CreateScope();
        var dbBefore = scopeBefore.ServiceProvider.GetRequiredService<OpsSphereDbContext>();
        var resolutionBefore = await dbBefore.TicketResolutions.AsNoTracking().SingleAsync(r => r.TicketId == ticket.Id);

        var response = await CloseAsync(agent, ticket.Id);

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        using var scopeAfter = factory.Services.CreateScope();
        var dbAfter = scopeAfter.ServiceProvider.GetRequiredService<OpsSphereDbContext>();
        var resolutionAfter = await dbAfter.TicketResolutions.AsNoTracking().SingleAsync(r => r.TicketId == ticket.Id);

        Assert.Equal(resolutionBefore.Id, resolutionAfter.Id);
        Assert.Equal(resolutionBefore.ResolutionSummary, resolutionAfter.ResolutionSummary);
        Assert.Equal(resolutionBefore.FinalSlaState, resolutionAfter.FinalSlaState);
    }

    // -------------------------------------------------------------------------
    // GetTicketHistory — Authorization and behavior
    // -------------------------------------------------------------------------

    [Fact]
    public async Task GetHistory_Agent_WithinScope_Returns200_WithEntries()
    {
        await using var factory = await TicketResolveCloseApiFactory.CreateAsync();
        var agent = await CreateAuthenticatedClientAsync(factory, AgentEmail);
        var ticket = await CreateNovaBankTicketAsync(agent, "History entries check");

        var response = await GetHistoryAsync(agent, ticket.Id);

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var history = await ReadDataAsync<IReadOnlyList<TicketStatusHistoryItemDto>>(response);
        Assert.NotEmpty(history);
    }

    [Fact]
    public async Task GetHistory_CrossScope_Returns404()
    {
        await using var factory = await TicketResolveCloseApiFactory.CreateAsync();
        var supervisor = await CreateAuthenticatedClientAsync(factory, SupervisorEmail);
        var outOfScopeTicketId = await AddTicketDirectlyAsync(factory, SeedIds.Accounts.Streamly, SeedIds.Campaigns.StreamlyCreator, SeedIds.Customers.StreamlyCustomer1, TicketStatus.Open, "Cross-scope history");

        var response = await GetHistoryAsync(supervisor, outOfScopeTicketId);

        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }

    [Fact]
    public async Task GetHistory_Admin_Returns403()
    {
        await using var factory = await TicketResolveCloseApiFactory.CreateAsync();
        var supervisor = await CreateAuthenticatedClientAsync(factory, SupervisorEmail);
        var admin = await CreateAuthenticatedClientAsync(factory, AdminEmail);
        var ticket = await CreateNovaBankTicketAsync(supervisor, "Admin history forbidden");

        var response = await GetHistoryAsync(admin, ticket.Id);

        Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);
    }

    [Fact]
    public async Task GetHistory_ReturnsEntriesInChronologicalOrder()
    {
        await using var factory = await TicketResolveCloseApiFactory.CreateAsync();
        var agent = await CreateAuthenticatedClientAsync(factory, AgentEmail);
        var ticket = await CreateNovaBankTicketAsync(agent, "History chronological order");

        await ResolveAsync(agent, ticket.Id, "Resolved for history order.", null);

        var response = await GetHistoryAsync(agent, ticket.Id);

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var history = await ReadDataAsync<IReadOnlyList<TicketStatusHistoryItemDto>>(response);
        Assert.True(history.Count >= 2);
        for (var i = 1; i < history.Count; i++)
        {
            Assert.True(
                DateTime.Parse(history[i].CreatedAt) >= DateTime.Parse(history[i - 1].CreatedAt),
                "History entries must be sorted ascending by CreatedAt.");
        }
    }

    [Fact]
    public async Task GetHistory_ReflectsResolveAndClose()
    {
        await using var factory = await TicketResolveCloseApiFactory.CreateAsync();
        var agent = await CreateAuthenticatedClientAsync(factory, AgentEmail);
        var ticket = await CreateNovaBankTicketAsync(agent, "History full lifecycle");

        await ResolveAsync(agent, ticket.Id, "Full lifecycle resolved.", null);
        await CloseAsync(agent, ticket.Id);

        var response = await GetHistoryAsync(agent, ticket.Id);

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var history = await ReadDataAsync<IReadOnlyList<TicketStatusHistoryItemDto>>(response);
        Assert.Contains(history, h => h.NewStatus == "Resolved");
        Assert.Contains(history, h => h.NewStatus == "Closed");
    }

    [Fact]
    public async Task Unauthenticated_CannotGetHistory_Returns401()
    {
        await using var factory = await TicketResolveCloseApiFactory.CreateAsync();
        var supervisor = await CreateAuthenticatedClientAsync(factory, SupervisorEmail);
        var ticket = await CreateNovaBankTicketAsync(supervisor, "Unauthenticated history");
        var anonymous = factory.CreateClient();

        var response = await GetHistoryAsync(anonymous, ticket.Id);

        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    // -------------------------------------------------------------------------
    // Full Lifecycle
    // -------------------------------------------------------------------------

    [Fact]
    public async Task FullLifecycle_OpenToResolvedToClosed_PersistsAll()
    {
        await using var factory = await TicketResolveCloseApiFactory.CreateAsync();
        var agent = await CreateAuthenticatedClientAsync(factory, AgentEmail);
        var ticket = await CreateNovaBankTicketAsync(agent, "Full lifecycle open to closed");

        var resolveResponse = await ResolveAsync(agent, ticket.Id, "Customer issue resolved.", "RC-FULL");
        Assert.Equal(HttpStatusCode.OK, resolveResponse.StatusCode);
        var resolveResult = await ReadDataAsync<ResolveTicketResponse>(resolveResponse);
        Assert.Equal("Resolved", resolveResult.NewStatus);

        var closeResponse = await CloseAsync(agent, ticket.Id);
        Assert.Equal(HttpStatusCode.OK, closeResponse.StatusCode);
        var closeResult = await ReadDataAsync<CloseTicketResponse>(closeResponse);
        Assert.Equal("Closed", closeResult.NewStatus);

        using var scope = factory.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<OpsSphereDbContext>();
        var dbTicket = await db.Tickets.AsNoTracking().SingleAsync(t => t.Id == ticket.Id);
        Assert.Equal(TicketStatus.Closed, dbTicket.Status);
        Assert.NotNull(dbTicket.ResolvedAt);
        Assert.NotNull(dbTicket.ClosedAt);

        var resolution = await db.TicketResolutions.AsNoTracking().SingleOrDefaultAsync(r => r.TicketId == ticket.Id);
        Assert.NotNull(resolution);
        Assert.Equal("Customer issue resolved.", resolution.ResolutionSummary);
        Assert.Equal("RC-FULL", resolution.ResolutionCode);
    }

    [Fact]
    public async Task ClosedTicket_CannotBeResolved_Returns400()
    {
        await using var factory = await TicketResolveCloseApiFactory.CreateAsync();
        var agent = await CreateAuthenticatedClientAsync(factory, AgentEmail);
        var ticket = await CreateNovaBankTicketAsync(agent, "Closed cannot be re-resolved");
        await AddResolutionDirectlyAsync(factory, ticket.Id, SeedIds.Users.AgentNovabank, "Already resolved", "WithinSla");
        await CloseAsync(agent, ticket.Id);

        var response = await ResolveAsync(agent, ticket.Id, "Try resolve closed.", null);

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        var error = await ReadErrorAsync(response);
        Assert.Equal("business_rule_violation", error.Error.Code);
    }

    [Fact]
    public async Task ClosedTicket_CannotBeClosed_Returns400()
    {
        await using var factory = await TicketResolveCloseApiFactory.CreateAsync();
        var agent = await CreateAuthenticatedClientAsync(factory, AgentEmail);
        var ticket = await CreateNovaBankTicketAsync(agent, "Closed cannot be re-closed");
        await AddResolutionDirectlyAsync(factory, ticket.Id, SeedIds.Users.AgentNovabank, "Already resolved", "WithinSla");
        await CloseAsync(agent, ticket.Id);

        var response = await CloseAsync(agent, ticket.Id);

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        var error = await ReadErrorAsync(response);
        Assert.Equal("business_rule_violation", error.Error.Code);
    }

    // -------------------------------------------------------------------------
    // HTTP helpers
    // -------------------------------------------------------------------------

    private static Task<HttpResponseMessage> ResolveAsync(HttpClient client, Guid ticketId, string? resolutionSummary, string? resolutionCode) =>
        client.PostAsJsonAsync($"/api/tickets/{ticketId}/resolve", new
        {
            resolutionSummary,
            resolutionCode
        });

    private static Task<HttpResponseMessage> CloseAsync(HttpClient client, Guid ticketId) =>
        client.PostAsJsonAsync($"/api/tickets/{ticketId}/close", new { });

    private static Task<HttpResponseMessage> GetHistoryAsync(HttpClient client, Guid ticketId) =>
        client.GetAsync($"/api/tickets/{ticketId}/history");

    // -------------------------------------------------------------------------
    // Arrange helpers
    // -------------------------------------------------------------------------

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
            customerId, accountId, campaignId, category, priority, subject, description
        });
        Assert.Equal(HttpStatusCode.Created, response.StatusCode);
        return await ReadDataAsync<CreateTicketResult>(response);
    }

    private static async Task<Guid> AddTicketDirectlyAsync(
        TicketResolveCloseApiFactory factory,
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

    private static async Task AddResolutionDirectlyAsync(
        TicketResolveCloseApiFactory factory,
        Guid ticketId,
        Guid resolvedByUserId,
        string summary,
        string finalSlaState)
    {
        using var scope = factory.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<OpsSphereDbContext>();
        var ticket = await db.Tickets.Include(t => t.SlaStateRecord).SingleAsync(t => t.Id == ticketId);
        var now = DateTime.UtcNow;

        ticket.Status = TicketStatus.Resolved;
        ticket.ResolvedAt = now;
        ticket.SlaState = SlaState.Completed;
        ticket.IsEscalated = false;
        ticket.UpdatedAt = now;

        if (ticket.SlaStateRecord is not null)
        {
            ticket.SlaStateRecord.State = SlaState.Completed.ToString();
            ticket.SlaStateRecord.FinalState = finalSlaState;
            ticket.SlaStateRecord.CompletedAt = now;
        }

        db.TicketResolutions.Add(new TicketResolution
        {
            Id = Guid.NewGuid(),
            TicketId = ticketId,
            ResolvedByUserId = resolvedByUserId,
            ResolutionSummary = summary,
            ResolutionCode = null,
            FinalSlaState = finalSlaState,
            CreatedAt = now
        });

        await db.SaveChangesAsync();
    }

    private static async Task SetTicketStatusAsync(TicketResolveCloseApiFactory factory, Guid ticketId, TicketStatus status)
    {
        using var scope = factory.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<OpsSphereDbContext>();
        var ticket = await db.Tickets.SingleAsync(t => t.Id == ticketId);
        ticket.Status = status;
        ticket.IsEscalated = status == TicketStatus.Escalated || ticket.IsEscalated;
        await db.SaveChangesAsync();
    }

    private static async Task SetTicketSlaStateAsync(TicketResolveCloseApiFactory factory, Guid ticketId, SlaState slaState, DateTime? dueAt = null)
    {
        using var scope = factory.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<OpsSphereDbContext>();
        var ticket = await db.Tickets.Include(t => t.SlaStateRecord).SingleAsync(t => t.Id == ticketId);
        ticket.SlaState = slaState;
        if (dueAt.HasValue) ticket.SlaDueAt = dueAt;

        if (ticket.SlaStateRecord is not null)
        {
            ticket.SlaStateRecord.State = slaState.ToString();
            if (dueAt.HasValue) ticket.SlaStateRecord.DueAt = dueAt.Value;
        }

        await db.SaveChangesAsync();
    }

    private static async Task AddEscalationDirectlyAsync(
        TicketResolveCloseApiFactory factory,
        Guid ticketId,
        Guid escalatedByUserId,
        string reason)
    {
        using var scope = factory.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<OpsSphereDbContext>();
        var ticket = await db.Tickets.SingleAsync(t => t.Id == ticketId);
        var now = DateTime.UtcNow;
        ticket.Status = TicketStatus.Escalated;
        ticket.IsEscalated = true;
        ticket.UpdatedAt = now;

        db.TicketEscalations.Add(new TicketEscalation
        {
            Id = Guid.NewGuid(),
            TicketId = ticketId,
            EscalatedByUserId = escalatedByUserId,
            EscalationReason = reason,
            ReviewedByUserId = null,
            ReviewNotes = null,
            ResolvedAt = null,
            IsActive = true,
            CreatedAt = now
        });
        await db.SaveChangesAsync();
    }

    // -------------------------------------------------------------------------
    // Read helpers
    // -------------------------------------------------------------------------

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

    private static async Task<HttpClient> CreateAuthenticatedClientAsync(TicketResolveCloseApiFactory factory, string email)
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

    // -------------------------------------------------------------------------
    // Response DTOs (test-local)
    // -------------------------------------------------------------------------

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

    private sealed record ResolveTicketResponse(
        Guid TicketId,
        string TicketNumber,
        Guid ResolutionId,
        string PreviousStatus,
        string NewStatus,
        string FinalSlaState,
        DateTime ResolvedAt,
        string Message);

    private sealed record CloseTicketResponse(
        Guid TicketId,
        string TicketNumber,
        string PreviousStatus,
        string NewStatus,
        DateTime ClosedAt,
        string Message);

    private sealed record TicketStatusHistoryItemDto(
        Guid Id,
        Guid TicketId,
        string? PreviousStatus,
        string NewStatus,
        Guid ChangedByUserId,
        string? ChangeReason,
        string CreatedAt);

    // -------------------------------------------------------------------------
    // Factory
    // -------------------------------------------------------------------------

    internal sealed class TicketResolveCloseApiFactory : WebApplicationFactory<Program>
    {
        public const string JwtSigningKey = "integration-testing-only-fictional-jwt-signing-key";

        private readonly SqliteConnection connection = new("Data Source=:memory:");

        public static async Task<TicketResolveCloseApiFactory> CreateAsync()
        {
            ConfigureEnvironment();
            var factory = new TicketResolveCloseApiFactory();
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
                    ["ConnectionStrings:DefaultConnection"] = "Server=(local);Database=OpsSphereResolveCloseTests;Trusted_Connection=True;TrustServerCertificate=True;",
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
            Environment.SetEnvironmentVariable("ConnectionStrings__DefaultConnection", "Server=(local);Database=OpsSphereResolveCloseTests;Trusted_Connection=True;TrustServerCertificate=True;");
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
