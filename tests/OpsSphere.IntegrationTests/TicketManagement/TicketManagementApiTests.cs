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
using OpsSphere.Infrastructure.Persistence;
using OpsSphere.Infrastructure.Persistence.SeedData;

namespace OpsSphere.IntegrationTests.TicketManagement;

public sealed class TicketManagementApiTests
{
    private const string AdminEmail = "admin@opssphere.local";
    private const string AgentEmail = "agent.novabank@opssphere.local";
    private const string SupervisorEmail = "supervisor.novabank@opssphere.local";
    private const string ViewerEmail = "viewer.latam@opssphere.local";

    private static readonly JsonSerializerOptions JsonOptions = new(JsonSerializerDefaults.Web);

    // ─── Happy path and authorization ────────────────────────────────────────

    [Fact]
    public async Task Agent_CanCreateTicket_Returns201_WithTicketNumber()
    {
        await using var factory = await TicketApiFactory.CreateAsync();
        var agent = await CreateAuthenticatedClientAsync(factory, AgentEmail);

        var response = await PostTicketAsync(agent,
            customerId: SeedIds.Customers.NovaBankCustomer1,
            accountId: SeedIds.Accounts.NovaBank,
            campaignId: SeedIds.Campaigns.NovaBankCreditCard,
            category: "Billing",
            priority: "Normal",
            subject: "Agent test ticket",
            description: "Testing agent ticket creation");

        Assert.Equal(HttpStatusCode.Created, response.StatusCode);

        var result = await ReadDataAsync<CreateTicketResult>(response);
        Assert.Equal("Open", result.Status);
        Assert.Matches(@"^OPS-\d{8}-\d{6}$", result.TicketNumber);
    }

    [Fact]
    public async Task Supervisor_CanCreateTicket_WithinAccountScope_Returns201()
    {
        await using var factory = await TicketApiFactory.CreateAsync();
        var supervisor = await CreateAuthenticatedClientAsync(factory, SupervisorEmail);

        // Supervisor has NOVABANK account scope — NovaBankFraud is within scope
        var response = await PostTicketAsync(supervisor,
            customerId: SeedIds.Customers.NovaBankCustomer1,
            accountId: SeedIds.Accounts.NovaBank,
            campaignId: SeedIds.Campaigns.NovaBankFraud,
            category: "Fraud",
            priority: "High",
            subject: "Supervisor fraud ticket",
            description: "Testing supervisor ticket creation with account scope");

        Assert.Equal(HttpStatusCode.Created, response.StatusCode);

        var result = await ReadDataAsync<CreateTicketResult>(response);
        Assert.Equal("Open", result.Status);
    }

    [Fact]
    public async Task Admin_CannotCreateTicket_Returns403()
    {
        await using var factory = await TicketApiFactory.CreateAsync();
        var admin = await CreateAuthenticatedClientAsync(factory, AdminEmail);

        var response = await PostTicketAsync(admin,
            customerId: SeedIds.Customers.NovaBankCustomer1,
            accountId: SeedIds.Accounts.NovaBank,
            campaignId: SeedIds.Campaigns.NovaBankCreditCard,
            category: "Billing",
            priority: "Normal",
            subject: "Admin forbidden",
            description: "Admin does not have tickets.create permission");

        Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);
    }

    [Fact]
    public async Task Viewer_CannotCreateTicket_Returns403()
    {
        await using var factory = await TicketApiFactory.CreateAsync();
        var viewer = await CreateAuthenticatedClientAsync(factory, ViewerEmail);

        var response = await PostTicketAsync(viewer,
            customerId: SeedIds.Customers.NovaBankCustomer1,
            accountId: SeedIds.Accounts.NovaBank,
            campaignId: SeedIds.Campaigns.NovaBankCreditCard,
            category: "Billing",
            priority: "Normal",
            subject: "Viewer forbidden",
            description: "Viewer does not have tickets.create permission");

        Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);
    }

    // ─── Validation ───────────────────────────────────────────────────────────

    [Fact]
    public async Task CreateTicket_MissingRequiredFields_Returns400_WithFieldErrors()
    {
        await using var factory = await TicketApiFactory.CreateAsync();
        var agent = await CreateAuthenticatedClientAsync(factory, AgentEmail);

        var response = await agent.PostAsJsonAsync("/api/tickets", new
        {
            customerId = SeedIds.Customers.NovaBankCustomer1,
            accountId = SeedIds.Accounts.NovaBank,
            campaignId = SeedIds.Campaigns.NovaBankCreditCard,
            category = (string?)null,
            priority = (string?)null,
            subject = (string?)null,
            description = (string?)null
        });

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        var error = await ReadErrorAsync(response);
        Assert.Equal("validation_error", error.Error.Code);
        Assert.Contains(error.Error.Details, d => d.Field == "category");
        Assert.Contains(error.Error.Details, d => d.Field == "subject");
        Assert.Contains(error.Error.Details, d => d.Field == "description");
        Assert.Contains(error.Error.Details, d => d.Field == "priority");
    }

    [Fact]
    public async Task CreateTicket_InvalidPriority_Returns400()
    {
        await using var factory = await TicketApiFactory.CreateAsync();
        var agent = await CreateAuthenticatedClientAsync(factory, AgentEmail);

        var response = await agent.PostAsJsonAsync("/api/tickets", new
        {
            customerId = SeedIds.Customers.NovaBankCustomer1,
            accountId = SeedIds.Accounts.NovaBank,
            campaignId = SeedIds.Campaigns.NovaBankCreditCard,
            category = "Billing",
            priority = "InvalidPriority",
            subject = "Bad priority test",
            description = "This should fail validation"
        });

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        var error = await ReadErrorAsync(response);
        Assert.Equal("validation_error", error.Error.Code);
        Assert.Contains(error.Error.Details, d => d.Field == "priority");
    }

    [Fact]
    public async Task CreateTicket_EmptyCampaignId_Returns400()
    {
        await using var factory = await TicketApiFactory.CreateAsync();
        var agent = await CreateAuthenticatedClientAsync(factory, AgentEmail);

        var response = await agent.PostAsJsonAsync("/api/tickets", new
        {
            customerId = SeedIds.Customers.NovaBankCustomer1,
            accountId = SeedIds.Accounts.NovaBank,
            campaignId = Guid.Empty,
            category = "Billing",
            priority = "Normal",
            subject = "Empty campaign id test",
            description = "This should fail because campaignId is empty"
        });

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        var error = await ReadErrorAsync(response);
        Assert.Equal("validation_error", error.Error.Code);
        Assert.Contains(error.Error.Details, d => d.Field == "campaignId");
    }

    // ─── Context validation ───────────────────────────────────────────────────

    [Fact]
    public async Task CreateTicket_CrossAccountCustomer_Returns400()
    {
        await using var factory = await TicketApiFactory.CreateAsync();
        var agent = await CreateAuthenticatedClientAsync(factory, AgentEmail);

        // StreamlyCustomer1 belongs to STREAMLY, but NovaBankCreditCard is under NOVABANK
        var response = await PostTicketAsync(agent,
            customerId: SeedIds.Customers.StreamlyCustomer1,
            accountId: SeedIds.Accounts.NovaBank,
            campaignId: SeedIds.Campaigns.NovaBankCreditCard,
            category: "Billing",
            priority: "Normal",
            subject: "Cross-account customer test",
            description: "StreamlyCustomer1 does not belong to the NovaBank campaign account");

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        var error = await ReadErrorAsync(response);
        Assert.Equal("validation_error", error.Error.Code);
        Assert.Contains(error.Error.Details, d => d.Field == "customerId");
    }

    [Fact]
    public async Task CreateTicket_OutOfScopeCampaign_Returns404()
    {
        await using var factory = await TicketApiFactory.CreateAsync();
        // Agent has campaign scope NOVABANK-CC only; NovaBankFraud is a different campaign in the same account
        // but the agent's scope is campaign-level, not account-level, so NovaBankFraud is out of scope
        var agent = await CreateAuthenticatedClientAsync(factory, AgentEmail);

        var response = await PostTicketAsync(agent,
            customerId: SeedIds.Customers.NovaBankCustomer1,
            accountId: SeedIds.Accounts.NovaBank,
            campaignId: SeedIds.Campaigns.NovaBankFraud,
            category: "Fraud",
            priority: "High",
            subject: "Out-of-scope campaign test",
            description: "Agent with NOVABANK-CC scope cannot access NovaBankFraud campaign");

        // Scope check returns 404 per existing convention
        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }

    // ─── Ticket side effects ──────────────────────────────────────────────────

    [Fact]
    public async Task CreatedTicket_HasStatus_Open()
    {
        await using var factory = await TicketApiFactory.CreateAsync();
        var agent = await CreateAuthenticatedClientAsync(factory, AgentEmail);

        var created = await CreateTicketAsync(agent,
            customerId: SeedIds.Customers.NovaBankCustomer1,
            accountId: SeedIds.Accounts.NovaBank,
            campaignId: SeedIds.Campaigns.NovaBankCreditCard,
            category: "Support",
            priority: "Low",
            subject: "Status check ticket",
            description: "Verifying initial ticket status is Open");

        var getResponse = await agent.GetAsync($"/api/tickets/{created.Id}");
        Assert.Equal(HttpStatusCode.OK, getResponse.StatusCode);

        var detail = await ReadDataAsync<TicketDetailDto>(getResponse);
        Assert.Equal("Open", detail.Status);
    }

    [Fact]
    public async Task CreatedTicket_HasSlaState_WithinSla_AndSlaDueAt_Set()
    {
        await using var factory = await TicketApiFactory.CreateAsync();
        var agent = await CreateAuthenticatedClientAsync(factory, AgentEmail);

        var created = await CreateTicketAsync(agent,
            customerId: SeedIds.Customers.NovaBankCustomer1,
            accountId: SeedIds.Accounts.NovaBank,
            campaignId: SeedIds.Campaigns.NovaBankCreditCard,
            category: "Support",
            priority: "Critical",
            subject: "SLA state check ticket",
            description: "Verifying initial SLA state and slaDueAt");

        // CreateTicketResult already contains slaState and slaDueAt
        Assert.Equal("WithinSla", created.SlaState);
        Assert.NotNull(created.SlaDueAt);

        // Also verify via GET detail endpoint
        var getResponse = await agent.GetAsync($"/api/tickets/{created.Id}");
        var detail = await ReadDataAsync<TicketDetailDto>(getResponse);
        Assert.Equal("WithinSla", detail.SlaState);
        Assert.NotNull(detail.SlaDueAt);
    }

    [Fact]
    public async Task CreatedTicket_HasInitialStatusHistoryRow()
    {
        await using var factory = await TicketApiFactory.CreateAsync();
        var agent = await CreateAuthenticatedClientAsync(factory, AgentEmail);

        var created = await CreateTicketAsync(agent,
            customerId: SeedIds.Customers.NovaBankCustomer1,
            accountId: SeedIds.Accounts.NovaBank,
            campaignId: SeedIds.Campaigns.NovaBankCreditCard,
            category: "Support",
            priority: "Normal",
            subject: "Status history check",
            description: "Verifying TicketStatusHistory row is created on ticket creation");

        using var scope = factory.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<OpsSphereDbContext>();

        var historyRow = await db.TicketStatusHistory
            .AsNoTracking()
            .FirstOrDefaultAsync(h => h.TicketId == created.Id);

        Assert.NotNull(historyRow);
        Assert.Equal("Open", historyRow.NewStatus);
        Assert.Null(historyRow.PreviousStatus);
    }

    [Fact]
    public async Task CreatedTicket_HasTicketSlaStateRow()
    {
        await using var factory = await TicketApiFactory.CreateAsync();
        var agent = await CreateAuthenticatedClientAsync(factory, AgentEmail);

        var created = await CreateTicketAsync(agent,
            customerId: SeedIds.Customers.NovaBankCustomer1,
            accountId: SeedIds.Accounts.NovaBank,
            campaignId: SeedIds.Campaigns.NovaBankCreditCard,
            category: "Support",
            priority: "High",
            subject: "SLA state row check",
            description: "Verifying TicketSlaState row is created on ticket creation");

        using var scope = factory.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<OpsSphereDbContext>();

        var slaRow = await db.TicketSlaStates
            .AsNoTracking()
            .FirstOrDefaultAsync(s => s.TicketId == created.Id);

        Assert.NotNull(slaRow);
        Assert.Equal("WithinSla", slaRow.State);
    }

    [Fact]
    public async Task CreatedTicket_HasAuditLog_WithAction_TicketCreated()
    {
        await using var factory = await TicketApiFactory.CreateAsync();
        var agent = await CreateAuthenticatedClientAsync(factory, AgentEmail);

        var created = await CreateTicketAsync(agent,
            customerId: SeedIds.Customers.NovaBankCustomer1,
            accountId: SeedIds.Accounts.NovaBank,
            campaignId: SeedIds.Campaigns.NovaBankCreditCard,
            category: "Billing",
            priority: "Normal",
            subject: "Audit log check",
            description: "Verifying audit log is written on ticket creation");

        await factory.AssertAuditAsync("TicketCreated", "Ticket", created.Id);
    }

    [Fact]
    public async Task AuditLog_DoesNotContain_TicketDescription_OrPii()
    {
        await using var factory = await TicketApiFactory.CreateAsync();
        var agent = await CreateAuthenticatedClientAsync(factory, AgentEmail);

        const string sensitiveDescription = "SensitiveDescription-PiiTestValue-XYZ987";
        const string sensitiveSubject = "SensitiveSubject-PiiTestValue-ABC123";

        var created = await CreateTicketAsync(agent,
            customerId: SeedIds.Customers.NovaBankCustomer1,
            accountId: SeedIds.Accounts.NovaBank,
            campaignId: SeedIds.Campaigns.NovaBankCreditCard,
            category: "Billing",
            priority: "Normal",
            subject: sensitiveSubject,
            description: sensitiveDescription);

        using var scope = factory.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<OpsSphereDbContext>();

        var auditLogs = await db.AuditLogs
            .AsNoTracking()
            .Where(log => log.EntityId == created.Id)
            .ToListAsync();

        Assert.NotEmpty(auditLogs);
        foreach (var log in auditLogs)
        {
            var combined = $"{log.PreviousValue}{log.NewValue}";
            Assert.DoesNotContain(sensitiveDescription, combined, StringComparison.OrdinalIgnoreCase);
            Assert.DoesNotContain(sensitiveSubject, combined, StringComparison.OrdinalIgnoreCase);
            // Customer name should not appear either
            Assert.DoesNotContain("Carlos", combined, StringComparison.OrdinalIgnoreCase);
            Assert.DoesNotContain("Mendez", combined, StringComparison.OrdinalIgnoreCase);
        }
    }

    // ─── Reads ────────────────────────────────────────────────────────────────

    [Fact]
    public async Task GetTickets_ScopedAgent_ReturnsOnly_CampaignTickets()
    {
        await using var factory = await TicketApiFactory.CreateAsync();
        var agent = await CreateAuthenticatedClientAsync(factory, AgentEmail);

        // Create a ticket in the agent's campaign scope
        var created = await CreateTicketAsync(agent,
            customerId: SeedIds.Customers.NovaBankCustomer1,
            accountId: SeedIds.Accounts.NovaBank,
            campaignId: SeedIds.Campaigns.NovaBankCreditCard,
            category: "Support",
            priority: "Normal",
            subject: "Scoped list test",
            description: "Verifying ticket appears in agent's scoped list");

        var listResponse = await agent.GetAsync("/api/tickets");
        Assert.Equal(HttpStatusCode.OK, listResponse.StatusCode);

        var tickets = await ReadDataAsync<IReadOnlyList<TicketListItemDto>>(listResponse);
        Assert.NotEmpty(tickets);
        Assert.Contains(tickets, t => t.Id == created.Id);

        // All returned tickets must belong to the campaign the agent has scope for (NOVABANK-CC = "Credit Card Support")
        Assert.All(tickets, t => Assert.Equal("Credit Card Support", t.CampaignName));
    }

    [Fact]
    public async Task GetTicket_CrossScope_Returns404()
    {
        await using var factory = await TicketApiFactory.CreateAsync();
        var agent = await CreateAuthenticatedClientAsync(factory, AgentEmail);
        var supervisor = await CreateAuthenticatedClientAsync(factory, SupervisorEmail);

        // Supervisor creates a ticket in NovaBankFraud (not visible to agent with NOVABANK-CC scope)
        var created = await CreateTicketAsync(supervisor,
            customerId: SeedIds.Customers.NovaBankCustomer1,
            accountId: SeedIds.Accounts.NovaBank,
            campaignId: SeedIds.Campaigns.NovaBankFraud,
            category: "Fraud",
            priority: "High",
            subject: "Cross-scope read test",
            description: "Supervisor creates in NovaBankFraud; Agent with NOVABANK-CC scope should get 404");

        // Agent with NOVABANK-CC scope cannot see a ticket in NovaBankFraud
        var getResponse = await agent.GetAsync($"/api/tickets/{created.Id}");
        Assert.Equal(HttpStatusCode.NotFound, getResponse.StatusCode);
    }

    [Fact]
    public async Task GetTickets_ReturnsEnvelopePattern()
    {
        await using var factory = await TicketApiFactory.CreateAsync();
        var agent = await CreateAuthenticatedClientAsync(factory, AgentEmail);

        var response = await agent.GetAsync("/api/tickets");
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);

        var json = await response.Content.ReadAsStringAsync();
        Assert.Contains("\"data\"", json, StringComparison.OrdinalIgnoreCase);

        var envelope = JsonSerializer.Deserialize<JsonElement>(json, JsonOptions);
        Assert.Equal(JsonValueKind.Array, envelope.GetProperty("data").ValueKind);
    }

    [Fact]
    public async Task TwoTickets_HaveDifferentTicketNumbers()
    {
        await using var factory = await TicketApiFactory.CreateAsync();
        var agent = await CreateAuthenticatedClientAsync(factory, AgentEmail);

        var first = await CreateTicketAsync(agent,
            customerId: SeedIds.Customers.NovaBankCustomer1,
            accountId: SeedIds.Accounts.NovaBank,
            campaignId: SeedIds.Campaigns.NovaBankCreditCard,
            category: "Billing",
            priority: "Low",
            subject: "First ticket",
            description: "First of two tickets for uniqueness test");

        var second = await CreateTicketAsync(agent,
            customerId: SeedIds.Customers.NovaBankCustomer1,
            accountId: SeedIds.Accounts.NovaBank,
            campaignId: SeedIds.Campaigns.NovaBankCreditCard,
            category: "Billing",
            priority: "Low",
            subject: "Second ticket",
            description: "Second of two tickets for uniqueness test");

        Assert.NotEqual(first.TicketNumber, second.TicketNumber);
        Assert.Matches(@"^OPS-\d{8}-\d{6}$", first.TicketNumber);
        Assert.Matches(@"^OPS-\d{8}-\d{6}$", second.TicketNumber);
    }

    // ─── Helpers ──────────────────────────────────────────────────────────────

    private static async Task<HttpResponseMessage> PostTicketAsync(
        HttpClient client, Guid customerId, Guid accountId, Guid campaignId,
        string category, string priority, string subject, string description)
    {
        return await client.PostAsJsonAsync("/api/tickets", new
        {
            customerId,
            accountId,
            campaignId,
            category,
            priority,
            subject,
            description
        });
    }

    private static async Task<CreateTicketResult> CreateTicketAsync(
        HttpClient client, Guid customerId, Guid accountId, Guid campaignId,
        string category, string priority, string subject, string description)
    {
        var response = await PostTicketAsync(client, customerId, accountId, campaignId, category, priority, subject, description);
        Assert.Equal(HttpStatusCode.Created, response.StatusCode);
        return await ReadDataAsync<CreateTicketResult>(response);
    }

    private static async Task<HttpClient> CreateAuthenticatedClientAsync(TicketApiFactory factory, string email)
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

    // ─── Response records ─────────────────────────────────────────────────────

    private sealed record ApiResponse<T>(T Data);
    private sealed record ApiErrorResponse(ApiError Error);
    private sealed record ApiError(string Code, string Message, IReadOnlyList<ApiErrorDetail> Details);
    private sealed record ApiErrorDetail(string? Field, string Message);
    private sealed record LoginData(string AccessToken);

    private sealed record CreateTicketResult(
        Guid Id,
        string TicketNumber,
        string Status,
        string Priority,
        string SlaState,
        DateTime? SlaDueAt);

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
        DateTime? UpdatedAt);

    private sealed record TicketListItemDto(
        Guid Id,
        string TicketNumber,
        string CustomerName,
        string AccountName,
        string CampaignName,
        string Priority,
        string Status,
        string SlaState,
        bool IsEscalated,
        DateTime CreatedAt);

    // ─── Factory ──────────────────────────────────────────────────────────────

    internal sealed class TicketApiFactory : WebApplicationFactory<Program>
    {
        public const string JwtSigningKey = "integration-testing-only-fictional-jwt-signing-key";

        private readonly SqliteConnection connection = new("Data Source=:memory:");

        public static async Task<TicketApiFactory> CreateAsync()
        {
            ConfigureEnvironment();
            var factory = new TicketApiFactory();
            await factory.connection.OpenAsync();
            await factory.InitializeDatabaseAsync();
            return factory;
        }

        public async Task AssertAuditAsync(string action, string entityType, Guid entityId)
        {
            using var scope = Services.CreateScope();
            var dbContext = scope.ServiceProvider.GetRequiredService<OpsSphereDbContext>();

            var audit = await dbContext.AuditLogs
                .AsNoTracking()
                .Where(log => log.Action == action && log.EntityType == entityType && log.EntityId == entityId)
                .OrderByDescending(log => log.CreatedAt)
                .FirstOrDefaultAsync();

            Assert.NotNull(audit);
            Assert.False(string.IsNullOrWhiteSpace(audit.CorrelationId));
            Assert.DoesNotContain("password", audit.PreviousValue ?? string.Empty, StringComparison.OrdinalIgnoreCase);
            Assert.DoesNotContain("password", audit.NewValue ?? string.Empty, StringComparison.OrdinalIgnoreCase);
            Assert.DoesNotContain(JwtSigningKey, audit.PreviousValue ?? string.Empty, StringComparison.Ordinal);
            Assert.DoesNotContain(JwtSigningKey, audit.NewValue ?? string.Empty, StringComparison.Ordinal);
        }

        protected override void ConfigureWebHost(IWebHostBuilder builder)
        {
            builder.UseEnvironment("Testing");
            builder.ConfigureAppConfiguration((_, config) =>
            {
                config.AddInMemoryCollection(new Dictionary<string, string?>
                {
                    ["ConnectionStrings:DefaultConnection"] = "Server=(local);Database=OpsSphereTicketTests;Trusted_Connection=True;TrustServerCertificate=True;",
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
            Environment.SetEnvironmentVariable("ConnectionStrings__DefaultConnection", "Server=(local);Database=OpsSphereTicketTests;Trusted_Connection=True;TrustServerCertificate=True;");
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
