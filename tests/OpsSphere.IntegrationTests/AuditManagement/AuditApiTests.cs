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
        await using var factory = await AuditApiFactory.CreateAsync();
        var ids = await ArrangeAuditLogsAsync(factory);
        var admin = await CreateAuthenticatedClientAsync(factory, AdminEmail);

        var response = await admin.GetAsync("/api/audit-logs");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var body = await ReadResponseAsync<PagedAuditListResponse>(response);
        Assert.Contains(body.Data, item => item.Id == ids.NovaBankTicketAuditId);
        Assert.Contains(body.Data, item => item.Id == ids.UserAuditId);
    }

    [Fact]
    public async Task OperationsManager_CanQueryAuditLogs_ScopedToRegion()
    {
        await using var factory = await AuditApiFactory.CreateAsync();
        var ids = await ArrangeAuditLogsAsync(factory);
        var manager = await CreateAuthenticatedClientAsync(factory, ManagerEmail);

        var response = await manager.GetAsync("/api/audit-logs");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var body = await ReadResponseAsync<PagedAuditListResponse>(response);
        Assert.Contains(body.Data, item => item.Id == ids.NovaBankTicketAuditId);
        Assert.DoesNotContain(body.Data, item => item.Id == ids.StreamlyTicketAuditId);
        Assert.DoesNotContain(body.Data, item => item.Id == ids.UserAuditId);
    }

    [Fact]
    public async Task Supervisor_CanQueryAuditLogs_ScopedToAccount()
    {
        await using var factory = await AuditApiFactory.CreateAsync();
        var ids = await ArrangeAuditLogsAsync(factory);
        var supervisor = await CreateAuthenticatedClientAsync(factory, SupervisorEmail);

        var response = await supervisor.GetAsync("/api/audit-logs");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var body = await ReadResponseAsync<PagedAuditListResponse>(response);
        Assert.Contains(body.Data, item => item.Id == ids.NovaBankTicketAuditId);
        Assert.Contains(body.Data, item => item.Id == ids.NovaBankCustomerAuditId);
        Assert.DoesNotContain(body.Data, item => item.Id == ids.StreamlyTicketAuditId);
        Assert.DoesNotContain(body.Data, item => item.Id == ids.UserAuditId);
    }

    [Fact]
    public async Task Agent_CannotQueryAuditLogs_Returns403()
    {
        await using var factory = await AuditApiFactory.CreateAsync();
        var agent = await CreateAuthenticatedClientAsync(factory, AgentEmail);

        var response = await agent.GetAsync("/api/audit-logs");

        Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);
    }

    [Fact]
    public async Task Viewer_CanQueryAuditLogs_ScopedToScope()
    {
        await using var factory = await AuditApiFactory.CreateAsync();
        var ids = await ArrangeAuditLogsAsync(factory);
        var viewer = await CreateAuthenticatedClientAsync(factory, ViewerEmail);

        var response = await viewer.GetAsync("/api/audit-logs");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var body = await ReadResponseAsync<PagedAuditListResponse>(response);
        Assert.Contains(body.Data, item => item.Id == ids.NovaBankTicketAuditId);
        Assert.DoesNotContain(body.Data, item => item.Id == ids.StreamlyTicketAuditId);
    }

    [Fact]
    public async Task Unauthenticated_CannotQueryAuditLogs_Returns401()
    {
        await using var factory = await AuditApiFactory.CreateAsync();

        var response = await factory.CreateClient().GetAsync("/api/audit-logs");

        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    [Fact]
    public async Task NonAdmin_CannotGetAuditLogById_OutOfScope_Returns404()
    {
        await using var factory = await AuditApiFactory.CreateAsync();
        var ids = await ArrangeAuditLogsAsync(factory);
        var supervisor = await CreateAuthenticatedClientAsync(factory, SupervisorEmail);

        var response = await supervisor.GetAsync($"/api/audit-logs/{ids.StreamlyTicketAuditId}");

        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }

    [Fact]
    public async Task Admin_CanGetAuditLogById_Returns200()
    {
        await using var factory = await AuditApiFactory.CreateAsync();
        var ids = await ArrangeAuditLogsAsync(factory);
        var admin = await CreateAuthenticatedClientAsync(factory, AdminEmail);

        var response = await admin.GetAsync($"/api/audit-logs/{ids.NovaBankTicketAuditId}");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var body = await ReadResponseAsync<ApiResponse<AuditDetailResponse>>(response);
        Assert.Equal(ids.NovaBankTicketAuditId, body.Data.Id);
        Assert.Equal("TicketCreated", body.Data.Action);
        Assert.NotNull(body.Data.NewValue);
        Assert.Equal("corr-ticket", body.Data.CorrelationId);
    }

    [Fact]
    public async Task NonAdmin_CanGetAuditLogById_InScope_Returns200()
    {
        await using var factory = await AuditApiFactory.CreateAsync();
        var ids = await ArrangeAuditLogsAsync(factory);
        var supervisor = await CreateAuthenticatedClientAsync(factory, SupervisorEmail);

        var response = await supervisor.GetAsync($"/api/audit-logs/{ids.NovaBankTicketAuditId}");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
    }

    [Fact]
    public async Task EntityAuditHistory_Ticket_IsScopedAndPaged()
    {
        await using var factory = await AuditApiFactory.CreateAsync();
        var ids = await ArrangeAuditLogsAsync(factory);
        var supervisor = await CreateAuthenticatedClientAsync(factory, SupervisorEmail);

        var response = await supervisor.GetAsync($"/api/audit-logs/entity/Ticket/{ids.NovaBankTicketId}");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var body = await ReadResponseAsync<PagedAuditListResponse>(response);
        Assert.Contains(body.Data, item => item.EntityId == ids.NovaBankTicketId);
        Assert.DoesNotContain(body.Data, item => item.EntityId == ids.StreamlyTicketId);
    }

    [Fact]
    public async Task Supervisor_CannotGetEntityAuditHistory_OutOfScope_Returns404()
    {
        await using var factory = await AuditApiFactory.CreateAsync();
        var ids = await ArrangeAuditLogsAsync(factory);
        var supervisor = await CreateAuthenticatedClientAsync(factory, SupervisorEmail);

        var response = await supervisor.GetAsync($"/api/audit-logs/entity/Ticket/{ids.StreamlyTicketId}");

        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }

    [Fact]
    public async Task AuditFilters_ReturnExpectedRows()
    {
        await using var factory = await AuditApiFactory.CreateAsync();
        var ids = await ArrangeAuditLogsAsync(factory);
        var admin = await CreateAuthenticatedClientAsync(factory, AdminEmail);

        var byActor = await ReadResponseAsync<PagedAuditListResponse>(
            await admin.GetAsync($"/api/audit-logs?actorUserId={SeedIds.Users.SupervisorNovabank}"));
        var byAction = await ReadResponseAsync<PagedAuditListResponse>(
            await admin.GetAsync("/api/audit-logs?action=CustomerUpdated"));
        var byEntity = await ReadResponseAsync<PagedAuditListResponse>(
            await admin.GetAsync($"/api/audit-logs?entityType=Ticket&entityId={ids.NovaBankTicketId}"));
        var byAccount = await ReadResponseAsync<PagedAuditListResponse>(
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
        await using var factory = await AuditApiFactory.CreateAsync();
        var ids = await ArrangeAuditLogsAsync(factory);
        await SetAuditCreatedAtAsync(factory, ids.NovaBankTicketAuditId, new DateTime(2026, 05, 01, 0, 0, 0, DateTimeKind.Utc));
        await SetAuditCreatedAtAsync(factory, ids.NovaBankCustomerAuditId, new DateTime(2026, 05, 10, 0, 0, 0, DateTimeKind.Utc));
        var admin = await CreateAuthenticatedClientAsync(factory, AdminEmail);

        var response = await admin.GetAsync("/api/audit-logs?fromUtc=2026-05-09T00:00:00Z&toUtc=2026-05-11T00:00:00Z");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var body = await ReadResponseAsync<PagedAuditListResponse>(response);
        Assert.Contains(body.Data, item => item.Id == ids.NovaBankCustomerAuditId);
        Assert.DoesNotContain(body.Data, item => item.Id == ids.NovaBankTicketAuditId);
    }

    [Fact]
    public async Task InvalidFilterDateRange_Returns400()
    {
        await using var factory = await AuditApiFactory.CreateAsync();
        var admin = await CreateAuthenticatedClientAsync(factory, AdminEmail);

        var response = await admin.GetAsync("/api/audit-logs?fromUtc=2026-05-11T00:00:00Z&toUtc=2026-05-09T00:00:00Z");

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    [Fact]
    public async Task AuditLogs_CannotBeModified_NoPutDeleteEndpoints()
    {
        await using var factory = await AuditApiFactory.CreateAsync();
        var ids = await ArrangeAuditLogsAsync(factory);
        var admin = await CreateAuthenticatedClientAsync(factory, AdminEmail);

        var put = await admin.PutAsJsonAsync($"/api/audit-logs/{ids.NovaBankTicketAuditId}", new { action = "Changed" });
        var delete = await admin.DeleteAsync($"/api/audit-logs/{ids.NovaBankTicketAuditId}");

        Assert.True(put.StatusCode is HttpStatusCode.NotFound or HttpStatusCode.MethodNotAllowed);
        Assert.True(delete.StatusCode is HttpStatusCode.NotFound or HttpStatusCode.MethodNotAllowed);
    }

    [Fact]
    public async Task TicketCreated_ProducesAuditRecord_QueryableViaApi()
    {
        await using var factory = await AuditApiFactory.CreateAsync();
        var supervisor = await CreateAuthenticatedClientAsync(factory, SupervisorEmail);

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
        var created = await ReadResponseAsync<ApiResponse<CreateTicketResponse>>(createResponse);

        var auditResponse = await supervisor.GetAsync($"/api/audit-logs?entityType=Ticket&entityId={created.Data.Id}&action=TicketCreated");

        Assert.Equal(HttpStatusCode.OK, auditResponse.StatusCode);
        var auditBody = await ReadResponseAsync<PagedAuditListResponse>(auditResponse);
        Assert.Single(auditBody.Data);
    }

    [Fact]
    public async Task AuditList_DoesNotExposeSensitiveDetailFields()
    {
        await using var factory = await AuditApiFactory.CreateAsync();
        await ArrangeAuditLogsAsync(factory);
        var admin = await CreateAuthenticatedClientAsync(factory, AdminEmail);

        var json = await (await admin.GetAsync("/api/audit-logs")).Content.ReadAsStringAsync();

        Assert.DoesNotContain("previousValue", json, StringComparison.OrdinalIgnoreCase);
        Assert.DoesNotContain("newValue", json, StringComparison.OrdinalIgnoreCase);
        Assert.DoesNotContain("ipAddress", json, StringComparison.OrdinalIgnoreCase);
        Assert.DoesNotContain("userAgent", json, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public async Task AuditDetail_ExposesPreviousNewValueAndCorrelationIdSafely()
    {
        await using var factory = await AuditApiFactory.CreateAsync();
        var ids = await ArrangeAuditLogsAsync(factory);
        var admin = await CreateAuthenticatedClientAsync(factory, AdminEmail);

        var response = await admin.GetAsync($"/api/audit-logs/{ids.NovaBankTicketAuditId}");

        var json = await response.Content.ReadAsStringAsync();
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        Assert.Contains("previousValue", json, StringComparison.OrdinalIgnoreCase);
        Assert.Contains("newValue", json, StringComparison.OrdinalIgnoreCase);
        Assert.Contains("correlationId", json, StringComparison.OrdinalIgnoreCase);
        Assert.DoesNotContain("ipAddress", json, StringComparison.OrdinalIgnoreCase);
        Assert.DoesNotContain("userAgent", json, StringComparison.OrdinalIgnoreCase);
    }

    private static async Task<AuditArrangement> ArrangeAuditLogsAsync(AuditApiFactory factory)
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
        AuditApiFactory factory,
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

    private static async Task SetAuditCreatedAtAsync(AuditApiFactory factory, Guid auditId, DateTime createdAt)
    {
        using var scope = factory.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<OpsSphereDbContext>();
        var audit = await db.AuditLogs.SingleAsync(a => a.Id == auditId);
        audit.CreatedAt = createdAt;
        await db.SaveChangesAsync();
    }

    private static async Task<HttpClient> CreateAuthenticatedClientAsync(AuditApiFactory factory, string email)
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

    private static async Task<T> ReadResponseAsync<T>(HttpResponseMessage response)
    {
        var json = await response.Content.ReadAsStringAsync();
        var value = JsonSerializer.Deserialize<T>(json, JsonOptions);
        return value ?? throw new InvalidOperationException($"Could not deserialize {typeof(T).Name} from {(int)response.StatusCode}: {json}");
    }

    private sealed record AuditArrangement(
        Guid NovaBankTicketId,
        Guid StreamlyTicketId,
        Guid NovaBankTicketAuditId,
        Guid StreamlyTicketAuditId,
        Guid NovaBankCustomerAuditId,
        Guid UserAuditId);

    private sealed record ApiResponse<T>(T Data);
    private sealed record PagedAuditListResponse(IReadOnlyList<AuditListItemResponse> Data, int Page, int PageSize, int TotalCount, int TotalPages);
    private sealed record AuditListItemResponse(Guid Id, Guid? ActorUserId, string? ActorDisplayName, string Action, string EntityType, Guid EntityId, DateTime CreatedAt, string? CorrelationId);
    private sealed record AuditDetailResponse(Guid Id, Guid? ActorUserId, string? ActorDisplayName, string Action, string EntityType, Guid EntityId, string? PreviousValue, string? NewValue, string? CorrelationId, DateTime CreatedAt);
    private sealed record LoginData(string AccessToken);
    private sealed record CreateTicketResponse(Guid Id, string TicketNumber, string Status, string Priority, string SlaState, DateTime? SlaDueAt);

    internal sealed class AuditApiFactory : WebApplicationFactory<Program>
    {
        public const string JwtSigningKey = "integration-testing-only-fictional-jwt-signing-key";

        private readonly SqliteConnection connection = new("Data Source=:memory:");

        public static async Task<AuditApiFactory> CreateAsync()
        {
            ConfigureEnvironment();
            var factory = new AuditApiFactory();
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
                    ["ConnectionStrings:DefaultConnection"] = "Server=(local);Database=OpsSphereAuditTests;Trusted_Connection=True;TrustServerCertificate=True;",
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
            Environment.SetEnvironmentVariable("ConnectionStrings__DefaultConnection", "Server=(local);Database=OpsSphereAuditTests;Trusted_Connection=True;TrustServerCertificate=True;");
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
