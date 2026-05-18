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
        await using var factory = await DashboardApiFactory.CreateAsync();

        var response = await factory.CreateClient().GetAsync("/api/dashboard/operational");

        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    [Fact]
    public async Task GetOperationalDashboard_WithAgent_Returns200()
    {
        await using var factory = await DashboardApiFactory.CreateAsync();
        var agent = await CreateAuthenticatedClientAsync(factory, AgentEmail);

        var response = await agent.GetAsync("/api/dashboard/operational");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
    }

    [Fact]
    public async Task GetOperationalDashboard_WithViewer_Returns200()
    {
        await using var factory = await DashboardApiFactory.CreateAsync();
        var viewer = await CreateAuthenticatedClientAsync(factory, ViewerEmail);

        var response = await viewer.GetAsync("/api/dashboard/operational");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
    }

    [Fact]
    public async Task GetOperationalDashboard_Manager_SeesOnlyRegionalTickets()
    {
        await using var factory = await DashboardApiFactory.CreateAsync();
        await AddTicketDirectlyAsync(factory, SeedIds.Accounts.NovaBank, SeedIds.Campaigns.NovaBankCreditCard, SeedIds.Customers.NovaBankCustomer1);
        await AddTicketDirectlyAsync(factory, SeedIds.Accounts.Streamly, SeedIds.Campaigns.StreamlyCreator, SeedIds.Customers.StreamlyCustomer1);
        var manager = await CreateAuthenticatedClientAsync(factory, ManagerEmail);

        var dashboard = await ReadDataAsync<OperationalDashboardResponse>(
            await manager.GetAsync("/api/dashboard/operational"));

        Assert.Equal(SeededNovaBankTicketCount + 1, dashboard.TotalTicketCount);
        Assert.Single(dashboard.TicketsByAccount);
        Assert.Equal(SeedIds.Accounts.NovaBank, dashboard.TicketsByAccount[0].EntityId);
    }

    [Fact]
    public async Task GetOperationalDashboard_Supervisor_SeesOnlyAccountScope()
    {
        await using var factory = await DashboardApiFactory.CreateAsync();
        await AddTicketDirectlyAsync(factory, SeedIds.Accounts.NovaBank, SeedIds.Campaigns.NovaBankCreditCard, SeedIds.Customers.NovaBankCustomer1);
        await AddTicketDirectlyAsync(factory, SeedIds.Accounts.Streamly, SeedIds.Campaigns.StreamlyCreator, SeedIds.Customers.StreamlyCustomer1);
        var supervisor = await CreateAuthenticatedClientAsync(factory, SupervisorEmail);

        var dashboard = await ReadDataAsync<OperationalDashboardResponse>(
            await supervisor.GetAsync("/api/dashboard/operational"));

        Assert.Equal(SeededNovaBankTicketCount + 1, dashboard.TotalTicketCount);
        Assert.Equal(SeedIds.Accounts.NovaBank, dashboard.TicketsByAccount[0].EntityId);
    }

    [Fact]
    public async Task GetOperationalDashboard_Agent_SeesOnlyCampaignScope()
    {
        await using var factory = await DashboardApiFactory.CreateAsync();
        await AddTicketDirectlyAsync(factory, SeedIds.Accounts.NovaBank, SeedIds.Campaigns.NovaBankCreditCard, SeedIds.Customers.NovaBankCustomer1);
        await AddTicketDirectlyAsync(factory, SeedIds.Accounts.NovaBank, SeedIds.Campaigns.NovaBankFraud, SeedIds.Customers.NovaBankCustomer1);
        var agent = await CreateAuthenticatedClientAsync(factory, AgentEmail);

        var dashboard = await ReadDataAsync<OperationalDashboardResponse>(
            await agent.GetAsync("/api/dashboard/operational"));

        Assert.Equal(SeededNovaBankTicketCount + 1, dashboard.TotalTicketCount);
        Assert.Equal(SeedIds.Campaigns.NovaBankCreditCard, dashboard.TicketsByCampaign[0].EntityId);
    }

    [Fact]
    public async Task GetOperationalDashboard_WithOutOfScopeAccountFilter_ReturnsZeroCounts()
    {
        await using var factory = await DashboardApiFactory.CreateAsync();
        await AddTicketDirectlyAsync(factory, SeedIds.Accounts.NovaBank, SeedIds.Campaigns.NovaBankCreditCard, SeedIds.Customers.NovaBankCustomer1);
        await AddTicketDirectlyAsync(factory, SeedIds.Accounts.Streamly, SeedIds.Campaigns.StreamlyCreator, SeedIds.Customers.StreamlyCustomer1);
        var supervisor = await CreateAuthenticatedClientAsync(factory, SupervisorEmail);

        var dashboard = await ReadDataAsync<OperationalDashboardResponse>(
            await supervisor.GetAsync($"/api/dashboard/operational?accountId={SeedIds.Accounts.Streamly}"));

        Assert.Equal(0, dashboard.TotalTicketCount);
        Assert.Empty(dashboard.TicketsByAccount);
    }

    [Fact]
    public async Task GetOperationalDashboard_Aggregates_AreCorrect()
    {
        await using var factory = await DashboardApiFactory.CreateAsync();
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
        var agent = await CreateAuthenticatedClientAsync(factory, AgentEmail);

        var dashboard = await ReadDataAsync<OperationalDashboardResponse>(
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
        await using var factory = await DashboardApiFactory.CreateAsync();
        await AddTicketDirectlyAsync(
            factory,
            SeedIds.Accounts.NovaBank,
            SeedIds.Campaigns.NovaBankCreditCard,
            SeedIds.Customers.NovaBankCustomer1,
            slaStartedAt: DateTime.UtcNow.AddHours(-6),
            slaDueAt: DateTime.UtcNow.AddHours(-1),
            storedSlaState: SlaState.WithinSla);
        var agent = await CreateAuthenticatedClientAsync(factory, AgentEmail);

        var dashboard = await ReadDataAsync<OperationalDashboardResponse>(
            await agent.GetAsync("/api/dashboard/operational"));

        Assert.Equal(3, dashboard.BreachedTicketCount);
        Assert.Contains(dashboard.TicketsBySlaState, item => item.Key == "Breached" && item.Count == 3);
    }

    [Fact]
    public async Task GetOperationalDashboard_WithAccountFilter_IntersectsScope()
    {
        await using var factory = await DashboardApiFactory.CreateAsync();
        await AddTicketDirectlyAsync(factory, SeedIds.Accounts.NovaBank, SeedIds.Campaigns.NovaBankCreditCard, SeedIds.Customers.NovaBankCustomer1);
        await AddTicketDirectlyAsync(factory, SeedIds.Accounts.NovaBank, SeedIds.Campaigns.NovaBankFraud, SeedIds.Customers.NovaBankCustomer1);
        var supervisor = await CreateAuthenticatedClientAsync(factory, SupervisorEmail);

        var dashboard = await ReadDataAsync<OperationalDashboardResponse>(
            await supervisor.GetAsync($"/api/dashboard/operational?accountId={SeedIds.Accounts.NovaBank}"));

        Assert.Equal(SeededNovaBankTicketCount + 2, dashboard.TotalTicketCount);
        Assert.All(dashboard.TicketsByAccount, item => Assert.Equal(SeedIds.Accounts.NovaBank, item.EntityId));
    }

    [Fact]
    public async Task GetOperationalDashboard_WithInvalidDateRange_Returns400()
    {
        await using var factory = await DashboardApiFactory.CreateAsync();
        var admin = await CreateAuthenticatedClientAsync(factory, AdminEmail);

        var response = await admin.GetAsync("/api/dashboard/operational?dateFrom=2026-05-18T00:00:00Z&dateTo=2026-05-17T00:00:00Z");

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    [Fact]
    public async Task GetOperationalDashboard_WithoutDashboardView_Returns403()
    {
        await using var factory = await DashboardApiFactory.CreateAsync();
        await RemoveDashboardPermissionFromAgentRoleAsync(factory);
        var agent = await CreateAuthenticatedClientAsync(factory, AgentEmail);

        var response = await agent.GetAsync("/api/dashboard/operational");

        Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);
    }

    private static async Task<Guid> AddTicketDirectlyAsync(
        DashboardApiFactory factory,
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

    private static async Task RemoveDashboardPermissionFromAgentRoleAsync(DashboardApiFactory factory)
    {
        using var scope = factory.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<OpsSphereDbContext>();
        var mapping = await db.RolePermissions
            .SingleAsync(rp => rp.RoleId == SeedIds.Roles.Agent && rp.PermissionId == SeedIds.Permissions.DashboardView);
        db.RolePermissions.Remove(mapping);
        await db.SaveChangesAsync();
    }

    private static async Task<HttpClient> CreateAuthenticatedClientAsync(DashboardApiFactory factory, string email)
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
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var envelope = await ReadResponseAsync<ApiResponse<T>>(response);
        return envelope.Data;
    }

    private static async Task<T> ReadResponseAsync<T>(HttpResponseMessage response)
    {
        var json = await response.Content.ReadAsStringAsync();
        var value = JsonSerializer.Deserialize<T>(json, JsonOptions);
        return value ?? throw new InvalidOperationException($"Could not deserialize {typeof(T).Name} from {(int)response.StatusCode}: {json}");
    }

    private sealed record ApiResponse<T>(T Data);
    private sealed record LoginData(string AccessToken);

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

    internal sealed class DashboardApiFactory : WebApplicationFactory<Program>
    {
        public const string JwtSigningKey = "integration-testing-only-fictional-jwt-signing-key";

        private readonly SqliteConnection connection = new("Data Source=:memory:");

        public static async Task<DashboardApiFactory> CreateAsync()
        {
            ConfigureEnvironment();
            var factory = new DashboardApiFactory();
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
                    ["ConnectionStrings:DefaultConnection"] = "Server=(local);Database=OpsSphereDashboardTests;Trusted_Connection=True;TrustServerCertificate=True;",
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
            Environment.SetEnvironmentVariable("ConnectionStrings__DefaultConnection", "Server=(local);Database=OpsSphereDashboardTests;Trusted_Connection=True;TrustServerCertificate=True;");
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
