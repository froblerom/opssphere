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

namespace OpsSphere.IntegrationTests.AuditManagement;

public sealed class AuditApiTests
{
    private const string AdminEmail = "admin@opssphere.local";
    private const string ManagerEmail = "manager.latam@opssphere.local";
    private const string SupervisorEmail = "supervisor.novabank@opssphere.local";
    private const string AgentEmail = "agent.novabank@opssphere.local";
    private const string ViewerEmail = "viewer.latam@opssphere.local";

    private static readonly JsonSerializerOptions JsonOptions = new(JsonSerializerDefaults.Web);

    [Fact]
    public async Task Admin_CanQueryAuditLogs_Returns200()
    {
        await using var factory = await OpsSphereSqliteFactory.CreateAsync();
        var ids = await ArrangeAuditLogsAsync(factory);
        var admin = await factory.CreateAuthenticatedClientAsync(AdminEmail);

        var response = await admin.GetAsync("/api/audit-logs");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var body = await OpsSphereSqliteFactory.ReadResponseAsync<PagedAuditListResponse>(response);
        Assert.Contains(body.Data, item => item.Id == ids.NovaBankTicketAuditId);
        Assert.Contains(body.Data, item => item.Id == ids.UserAuditId);
    }

    [Fact]
    public async Task OperationsManager_CanQueryAuditLogs_ScopedToRegion()
    {
        await using var factory = await OpsSphereSqliteFactory.CreateAsync();
        var ids = await ArrangeAuditLogsAsync(factory);
        var manager = await factory.CreateAuthenticatedClientAsync(ManagerEmail);

        var response = await manager.GetAsync("/api/audit-logs");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var body = await OpsSphereSqliteFactory.ReadResponseAsync<PagedAuditListResponse>(response);
        Assert.Contains(body.Data, item => item.Id == ids.NovaBankTicketAuditId);
        Assert.DoesNotContain(body.Data, item => item.Id == ids.StreamlyTicketAuditId);
        Assert.DoesNotContain(body.Data, item => item.Id == ids.UserAuditId);
    }

    [Fact]
    public async Task Supervisor_CanQueryAuditLogs_ScopedToAccount()
    {
        await using var factory = await OpsSphereSqliteFactory.CreateAsync();
        var ids = await ArrangeAuditLogsAsync(factory);
        var supervisor = await factory.CreateAuthenticatedClientAsync(SupervisorEmail);

        var response = await supervisor.GetAsync("/api/audit-logs");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var body = await OpsSphereSqliteFactory.ReadResponseAsync<PagedAuditListResponse>(response);
        Assert.Contains(body.Data, item => item.Id == ids.NovaBankTicketAuditId);
        Assert.Contains(body.Data, item => item.Id == ids.NovaBankCustomerAuditId);
        Assert.DoesNotContain(body.Data, item => item.Id == ids.StreamlyTicketAuditId);
        Assert.DoesNotContain(body.Data, item => item.Id == ids.UserAuditId);
    }

    [Fact]
    public async Task Agent_CannotQueryAuditLogs_Returns403()
    {
        await using var factory = await OpsSphereSqliteFactory.CreateAsync();
        var agent = await factory.CreateAuthenticatedClientAsync(AgentEmail);

        var response = await agent.GetAsync("/api/audit-logs");

        Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);
    }

    [Fact]
    public async Task Viewer_CanQueryAuditLogs_ScopedToScope()
    {
        await using var factory = await OpsSphereSqliteFactory.CreateAsync();
        var ids = await ArrangeAuditLogsAsync(factory);
        var viewer = await factory.CreateAuthenticatedClientAsync(ViewerEmail);

        var response = await viewer.GetAsync("/api/audit-logs");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var body = await OpsSphereSqliteFactory.ReadResponseAsync<PagedAuditListResponse>(response);
        Assert.Contains(body.Data, item => item.Id == ids.NovaBankTicketAuditId);
        Assert.DoesNotContain(body.Data, item => item.Id == ids.StreamlyTicketAuditId);
    }

    [Fact]
    public async Task Unauthenticated_CannotQueryAuditLogs_Returns401()
    {
        await using var factory = await OpsSphereSqliteFactory.CreateAsync();

        var response = await factory.CreateClient().GetAsync("/api/audit-logs");

        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    [Fact]
    public async Task NonAdmin_CannotGetAuditLogById_OutOfScope_Returns404()
    {
        await using var factory = await OpsSphereSqliteFactory.CreateAsync();
        var ids = await ArrangeAuditLogsAsync(factory);
        var supervisor = await factory.CreateAuthenticatedClientAsync(SupervisorEmail);

        var response = await supervisor.GetAsync($"/api/audit-logs/{ids.StreamlyTicketAuditId}");

        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }

    [Fact]
    public async Task Admin_CanGetAuditLogById_Returns200()
    {
        await using var factory = await OpsSphereSqliteFactory.CreateAsync();
        var ids = await ArrangeAuditLogsAsync(factory);
        var admin = await factory.CreateAuthenticatedClientAsync(AdminEmail);

        var response = await admin.GetAsync($"/api/audit-logs/{ids.NovaBankTicketAuditId}");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var body = await OpsSphereSqliteFactory.ReadResponseAsync<OpsSphereSqliteFactory.ApiResponse<AuditDetailResponse>>(response);
        Assert.Equal(ids.NovaBankTicketAuditId, body.Data.Id);
        Assert.Equal("TicketCreated", body.Data.Action);
        Assert.NotNull(body.Data.NewValue);
        Assert.Equal("corr-ticket", body.Data.CorrelationId);
    }

    [Fact]
    public async Task NonAdmin_CanGetAuditLogById_InScope_Returns200()
    {
        await using var factory = await OpsSphereSqliteFactory.CreateAsync();
        var ids = await ArrangeAuditLogsAsync(factory);
        var supervisor = await factory.CreateAuthenticatedClientAsync(SupervisorEmail);

        var response = await supervisor.GetAsync($"/api/audit-logs/{ids.NovaBankTicketAuditId}");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
    }

    [Fact]
    public async Task EntityAuditHistory_Ticket_IsScopedAndPaged()
    {
        await using var factory = await OpsSphereSqliteFactory.CreateAsync();
        var ids = await ArrangeAuditLogsAsync(factory);
        var supervisor = await factory.CreateAuthenticatedClientAsync(SupervisorEmail);

        var response = await supervisor.GetAsync($"/api/audit-logs/entity/Ticket/{ids.NovaBankTicketId}");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var body = await OpsSphereSqliteFactory.ReadResponseAsync<PagedAuditListResponse>(response);
        Assert.Contains(body.Data, item => item.EntityId == ids.NovaBankTicketId);
        Assert.DoesNotContain(body.Data, item => item.EntityId == ids.StreamlyTicketId);
    }

    [Fact]
    public async Task Supervisor_CannotGetEntityAuditHistory_OutOfScope_Returns404()
    {
        await using var factory = await OpsSphereSqliteFactory.CreateAsync();
        var ids = await ArrangeAuditLogsAsync(factory);
        var supervisor = await factory.CreateAuthenticatedClientAsync(SupervisorEmail);

        var response = await supervisor.GetAsync($"/api/audit-logs/entity/Ticket/{ids.StreamlyTicketId}");

        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }

    [Fact]
    public async Task AuditFilters_ReturnExpectedRows()
    {
        await using var factory = await OpsSphereSqliteFactory.CreateAsync();
        var ids = await ArrangeAuditLogsAsync(factory);
        var admin = await factory.CreateAuthenticatedClientAsync(AdminEmail);

        var byActor = await OpsSphereSqliteFactory.ReadResponseAsync<PagedAuditListResponse>(
            await admin.GetAsync($"/api/audit-logs?actorUserId={SeedIds.Users.SupervisorNovabank}"));
        var byAction = await OpsSphereSqliteFactory.ReadResponseAsync<PagedAuditListResponse>(
            await admin.GetAsync("/api/audit-logs?action=CustomerUpdated"));
        var byEntity = await OpsSphereSqliteFactory.ReadResponseAsync<PagedAuditListResponse>(
            await admin.GetAsync($"/api/audit-logs?entityType=Ticket&entityId={ids.NovaBankTicketId}"));
        var byAccount = await OpsSphereSqliteFactory.ReadResponseAsync<PagedAuditListResponse>(
            await admin.GetAsync($"/api/audit-logs?accountId={SeedIds.Accounts.NovaBank}"));

        Assert.Contains(byActor.Data, item => item.ActorUserId == SeedIds.Users.SupervisorNovabank);
        Assert.All(byAction.Data, item => Assert.Equal("CustomerUpdated", item.Action));
        Assert.Single(byEntity.Data);
        Assert.Contains(byAccount.Data, item => item.Id == ids.NovaBankTicketAuditId);
        Assert.DoesNotContain(byAccount.Data, item => item.Id == ids.StreamlyTicketAuditId);
    }

    [Fact]
    public async Task AuditFilters_ByDateRange_ReturnExpectedRows()
    {
        await using var factory = await OpsSphereSqliteFactory.CreateAsync();
        var ids = await ArrangeAuditLogsAsync(factory);
        await SetAuditCreatedAtAsync(factory, ids.NovaBankTicketAuditId, new DateTime(2026, 05, 01, 0, 0, 0, DateTimeKind.Utc));
        await SetAuditCreatedAtAsync(factory, ids.NovaBankCustomerAuditId, new DateTime(2026, 05, 10, 0, 0, 0, DateTimeKind.Utc));
        var admin = await factory.CreateAuthenticatedClientAsync(AdminEmail);

        var response = await admin.GetAsync("/api/audit-logs?fromUtc=2026-05-09T00:00:00Z&toUtc=2026-05-11T00:00:00Z");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var body = await OpsSphereSqliteFactory.ReadResponseAsync<PagedAuditListResponse>(response);
        Assert.Contains(body.Data, item => item.Id == ids.NovaBankCustomerAuditId);
        Assert.DoesNotContain(body.Data, item => item.Id == ids.NovaBankTicketAuditId);
    }

    [Fact]
    public async Task InvalidFilterDateRange_Returns400()
    {
        await using var factory = await OpsSphereSqliteFactory.CreateAsync();
        var admin = await factory.CreateAuthenticatedClientAsync(AdminEmail);

        var response = await admin.GetAsync("/api/audit-logs?fromUtc=2026-05-11T00:00:00Z&toUtc=2026-05-09T00:00:00Z");

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    [Fact]
    public async Task AuditLogs_CannotBeModified_NoPutDeleteEndpoints()
    {
        await using var factory = await OpsSphereSqliteFactory.CreateAsync();
        var ids = await ArrangeAuditLogsAsync(factory);
        var admin = await factory.CreateAuthenticatedClientAsync(AdminEmail);

        var put = await admin.PutAsJsonAsync($"/api/audit-logs/{ids.NovaBankTicketAuditId}", new { action = "Changed" });
        var delete = await admin.DeleteAsync($"/api/audit-logs/{ids.NovaBankTicketAuditId}");

        Assert.True(put.StatusCode is HttpStatusCode.NotFound or HttpStatusCode.MethodNotAllowed);
        Assert.True(delete.StatusCode is HttpStatusCode.NotFound or HttpStatusCode.MethodNotAllowed);
    }

    [Fact]
    public async Task TicketCreated_ProducesAuditRecord_QueryableViaApi()
    {
        await using var factory = await OpsSphereSqliteFactory.CreateAsync();
        var supervisor = await factory.CreateAuthenticatedClientAsync(SupervisorEmail);

        var createResponse = await supervisor.PostAsJsonAsync("/api/tickets", new
        {
            customerId = SeedIds.Customers.NovaBankCustomer1,
            accountId = SeedIds.Accounts.NovaBank,
            campaignId = SeedIds.Campaigns.NovaBankCreditCard,
            category = "Support",
            priority = "Normal",
            subject = "Queryable audit record",
            description = "Creates an audit log through the existing ticket write path."
        });
        createResponse.EnsureSuccessStatusCode();
        var created = await OpsSphereSqliteFactory.ReadResponseAsync<OpsSphereSqliteFactory.ApiResponse<CreateTicketResponse>>(createResponse);

        var auditResponse = await supervisor.GetAsync($"/api/audit-logs?entityType=Ticket&entityId={created.Data.Id}&action=TicketCreated");

        Assert.Equal(HttpStatusCode.OK, auditResponse.StatusCode);
        var auditBody = await OpsSphereSqliteFactory.ReadResponseAsync<PagedAuditListResponse>(auditResponse);
        Assert.Single(auditBody.Data);
    }

    [Fact]
    public async Task AuditList_DoesNotExposeSensitiveDetailFields()
    {
        await using var factory = await OpsSphereSqliteFactory.CreateAsync();
        await ArrangeAuditLogsAsync(factory);
        var admin = await factory.CreateAuthenticatedClientAsync(AdminEmail);

        var json = await (await admin.GetAsync("/api/audit-logs")).Content.ReadAsStringAsync();

        Assert.DoesNotContain("previousValue", json, StringComparison.OrdinalIgnoreCase);
        Assert.DoesNotContain("newValue", json, StringComparison.OrdinalIgnoreCase);
        Assert.DoesNotContain("ipAddress", json, StringComparison.OrdinalIgnoreCase);
        Assert.DoesNotContain("userAgent", json, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public async Task AuditDetail_ExposesPreviousNewValueAndCorrelationIdSafely()
    {
        await using var factory = await OpsSphereSqliteFactory.CreateAsync();
        var ids = await ArrangeAuditLogsAsync(factory);
        var admin = await factory.CreateAuthenticatedClientAsync(AdminEmail);

        var response = await admin.GetAsync($"/api/audit-logs/{ids.NovaBankTicketAuditId}");

        var json = await response.Content.ReadAsStringAsync();
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        Assert.Contains("previousValue", json, StringComparison.OrdinalIgnoreCase);
        Assert.Contains("newValue", json, StringComparison.OrdinalIgnoreCase);
        Assert.Contains("correlationId", json, StringComparison.OrdinalIgnoreCase);
        Assert.DoesNotContain("ipAddress", json, StringComparison.OrdinalIgnoreCase);
        Assert.DoesNotContain("userAgent", json, StringComparison.OrdinalIgnoreCase);
    }

    private static async Task<AuditArrangement> ArrangeAuditLogsAsync(OpsSphereSqliteFactory factory)
    {
        var novaTicketId = await AddTicketDirectlyAsync(
            factory,
            SeedIds.Accounts.NovaBank,
            SeedIds.Campaigns.NovaBankCreditCard,
            SeedIds.Customers.NovaBankCustomer1,
            "Nova audit ticket");
        var streamlyTicketId = await AddTicketDirectlyAsync(
            factory,
            SeedIds.Accounts.Streamly,
            SeedIds.Campaigns.StreamlyCreator,
            SeedIds.Customers.StreamlyCustomer1,
            "Streamly audit ticket");

        using var scope = factory.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<OpsSphereDbContext>();
        var now = DateTime.UtcNow;
        var novaTicketAuditId = Guid.NewGuid();
        var streamlyTicketAuditId = Guid.NewGuid();
        var novaCustomerAuditId = Guid.NewGuid();
        var userAuditId = Guid.NewGuid();

        db.AuditLogs.AddRange(
            new AuditLog
            {
                Id = novaTicketAuditId,
                ActorUserId = SeedIds.Users.SupervisorNovabank,
                Action = "TicketCreated",
                EntityType = "Ticket",
                EntityId = novaTicketId,
                PreviousValue = null,
                NewValue = "{\"status\":\"Open\"}",
                CorrelationId = "corr-ticket",
                CreatedAt = now
            },
            new AuditLog
            {
                Id = streamlyTicketAuditId,
                ActorUserId = SeedIds.Users.ManagerLatam,
                Action = "TicketCreated",
                EntityType = "Ticket",
                EntityId = streamlyTicketId,
                PreviousValue = null,
                NewValue = "{\"status\":\"Open\"}",
                CorrelationId = "corr-streamly",
                CreatedAt = now
            },
            new AuditLog
            {
                Id = novaCustomerAuditId,
                ActorUserId = SeedIds.Users.SupervisorNovabank,
                Action = "CustomerUpdated",
                EntityType = "Customer",
                EntityId = SeedIds.Customers.NovaBankCustomer1,
                PreviousValue = "{\"isActive\":true}",
                NewValue = "{\"isActive\":true}",
                CorrelationId = "corr-customer",
                CreatedAt = now
            },
            new AuditLog
            {
                Id = userAuditId,
                ActorUserId = SeedIds.Users.Admin,
                Action = "UserRolesUpdated",
                EntityType = "User",
                EntityId = SeedIds.Users.AgentNovabank,
                PreviousValue = "{}",
                NewValue = "{}",
                CorrelationId = "corr-user",
                CreatedAt = now
            });

        await db.SaveChangesAsync();
        return new AuditArrangement(novaTicketId, streamlyTicketId, novaTicketAuditId, streamlyTicketAuditId, novaCustomerAuditId, userAuditId);
    }

    private static async Task<Guid> AddTicketDirectlyAsync(
        OpsSphereSqliteFactory factory,
        Guid accountId,
        Guid campaignId,
        Guid customerId,
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
            TicketNumber = $"OPS-20990102-{Random.Shared.Next(1, 999999):D6}",
            CustomerId = customerId,
            RegionId = campaign.RegionId,
            CountryId = campaign.CountryId,
            AccountId = accountId,
            CampaignId = campaignId,
            CreatedByUserId = SeedIds.Users.SupervisorNovabank,
            Category = "Support",
            Priority = TicketPriority.Normal,
            Status = TicketStatus.Open,
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
            NewStatus = TicketStatus.Open.ToString(),
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

    private static async Task SetAuditCreatedAtAsync(OpsSphereSqliteFactory factory, Guid auditId, DateTime createdAt)
    {
        using var scope = factory.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<OpsSphereDbContext>();
        var audit = await db.AuditLogs.SingleAsync(a => a.Id == auditId);
        audit.CreatedAt = createdAt;
        await db.SaveChangesAsync();
    }

private sealed record AuditArrangement(
        Guid NovaBankTicketId,
        Guid StreamlyTicketId,
        Guid NovaBankTicketAuditId,
        Guid StreamlyTicketAuditId,
        Guid NovaBankCustomerAuditId,
        Guid UserAuditId);
    private sealed record PagedAuditListResponse(IReadOnlyList<AuditListItemResponse> Data, int Page, int PageSize, int TotalCount, int TotalPages);
    private sealed record AuditListItemResponse(Guid Id, Guid? ActorUserId, string? ActorDisplayName, string Action, string EntityType, Guid EntityId, DateTime CreatedAt, string? CorrelationId);
    private sealed record AuditDetailResponse(Guid Id, Guid? ActorUserId, string? ActorDisplayName, string Action, string EntityType, Guid EntityId, string? PreviousValue, string? NewValue, string? CorrelationId, DateTime CreatedAt);
    private sealed record CreateTicketResponse(Guid Id, string TicketNumber, string Status, string Priority, string SlaState, DateTime? SlaDueAt);
}