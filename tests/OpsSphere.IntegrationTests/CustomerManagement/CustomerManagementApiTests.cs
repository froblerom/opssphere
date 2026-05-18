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
using OpsSphere.Infrastructure.Persistence;
using OpsSphere.Infrastructure.Persistence.SeedData;

namespace OpsSphere.IntegrationTests.CustomerManagement;

public sealed class CustomerManagementApiTests
{
    private const string AdminEmail = "admin@opssphere.local";
    private const string AgentEmail = "agent.novabank@opssphere.local";
    private const string SupervisorEmail = "supervisor.novabank@opssphere.local";
    private const string ViewerEmail = "viewer.latam@opssphere.local";

    private static readonly JsonSerializerOptions JsonOptions = new(JsonSerializerDefaults.Web);

    [Fact]
    public async Task Admin_CanCreateUpdateAndDeactivateCustomer()
    {
        await using var factory = await CustomerApiFactory.CreateAsync();
        var client = await CreateAuthenticatedClientAsync(factory, AdminEmail);

        var customer = await CreateCustomerAsync(client, SeedIds.Accounts.NovaBank, "Lucia", "Reyes", "l.reyes@fictional.test", null, "NB-NEW-001");
        Assert.Equal("Lucia", customer.FirstName);
        Assert.Equal("Reyes", customer.LastName);
        Assert.Equal("NB-NEW-001", customer.ExternalReference);
        Assert.True(customer.IsActive);
        await factory.AssertAuditAsync("CustomerCreated", "Customer", customer.Id);

        var updateResponse = await client.PutAsJsonAsync($"/api/customers/{customer.Id}", new
        {
            accountId = SeedIds.Accounts.NovaBank,
            firstName = "Lucia",
            lastName = "Reyes-Updated",
            email = "l.reyes.updated@fictional.test",
            phoneNumber = (string?)null,
            externalReference = "NB-NEW-001-U"
        });
        var updated = await ReadDataAsync<CustomerResponse>(updateResponse);
        Assert.Equal(HttpStatusCode.OK, updateResponse.StatusCode);
        Assert.Equal("Reyes-Updated", updated.LastName);
        Assert.Equal("NB-NEW-001-U", updated.ExternalReference);
        await factory.AssertAuditAsync("CustomerUpdated", "Customer", customer.Id);

        var deactivateResponse = await client.PostAsync($"/api/customers/{customer.Id}/deactivate", null);
        Assert.Equal(HttpStatusCode.NoContent, deactivateResponse.StatusCode);
        await factory.AssertAuditAsync("CustomerDeactivated", "Customer", customer.Id);

        await AssertResponseDoesNotExposeSensitiveDataAsync(updateResponse);
    }

    [Fact]
    public async Task Viewer_CanReadCustomersButNotWriteThem()
    {
        await using var factory = await CustomerApiFactory.CreateAsync();
        var viewer = await CreateAuthenticatedClientAsync(factory, ViewerEmail);

        var listResponse = await viewer.GetAsync("/api/customers");
        Assert.Equal(HttpStatusCode.OK, listResponse.StatusCode);

        var createResponse = await viewer.PostAsJsonAsync("/api/customers", new
        {
            accountId = SeedIds.Accounts.NovaBank,
            firstName = "Denied",
            lastName = "Write",
            email = (string?)null,
            phoneNumber = (string?)null,
            externalReference = (string?)null
        });
        Assert.Equal(HttpStatusCode.Forbidden, createResponse.StatusCode);

        var updateResponse = await viewer.PutAsJsonAsync($"/api/customers/{SeedIds.Customers.NovaBankCustomer1}", new
        {
            accountId = SeedIds.Accounts.NovaBank,
            firstName = "Denied",
            lastName = "Update",
            email = (string?)null,
            phoneNumber = (string?)null,
            externalReference = (string?)null
        });
        Assert.Equal(HttpStatusCode.Forbidden, updateResponse.StatusCode);

        var deactivateResponse = await viewer.PostAsync($"/api/customers/{SeedIds.Customers.NovaBankCustomer1}/deactivate", null);
        Assert.Equal(HttpStatusCode.Forbidden, deactivateResponse.StatusCode);
    }

    [Fact]
    public async Task CustomerValidation_RejectsRequiredFieldFailures()
    {
        await using var factory = await CustomerApiFactory.CreateAsync();
        var client = await CreateAuthenticatedClientAsync(factory, AdminEmail);

        var missing = await client.PostAsJsonAsync("/api/customers", new
        {
            accountId = SeedIds.Accounts.NovaBank,
            firstName = "",
            lastName = "",
            email = (string?)null,
            phoneNumber = (string?)null,
            externalReference = (string?)null
        });
        var missingError = await ReadErrorAsync(missing);
        Assert.Equal(HttpStatusCode.BadRequest, missing.StatusCode);
        Assert.Equal("validation_error", missingError.Error.Code);
        Assert.Contains(missingError.Error.Details, d => d.Field == "firstName");
        Assert.Contains(missingError.Error.Details, d => d.Field == "lastName");
    }

    [Fact]
    public async Task CustomerCreate_RejectsInactiveOrMissingAccount()
    {
        await using var factory = await CustomerApiFactory.CreateAsync();
        var client = await CreateAuthenticatedClientAsync(factory, AdminEmail);

        var inactiveAccountId = await factory.AddInactiveAccountAsync("VOID-CA", "Void Customer Account");

        var missingAccount = await client.PostAsJsonAsync("/api/customers", new
        {
            accountId = Guid.NewGuid(),
            firstName = "Test",
            lastName = "Customer",
            email = (string?)null,
            phoneNumber = (string?)null,
            externalReference = (string?)null
        });
        await AssertValidationAsync(missingAccount, "accountId");

        var inactiveAccount = await client.PostAsJsonAsync("/api/customers", new
        {
            accountId = inactiveAccountId,
            firstName = "Test",
            lastName = "Customer",
            email = (string?)null,
            phoneNumber = (string?)null,
            externalReference = (string?)null
        });
        await AssertValidationAsync(inactiveAccount, "accountId");
    }

    [Fact]
    public async Task ScopedReads_FilterCustomersByAccountScope()
    {
        await using var factory = await CustomerApiFactory.CreateAsync();
        var agent = await CreateAuthenticatedClientAsync(factory, AgentEmail);
        var supervisor = await CreateAuthenticatedClientAsync(factory, SupervisorEmail);

        var agentCustomers = await ReadDataAsync<IReadOnlyList<CustomerResponse>>(await agent.GetAsync("/api/customers"));
        var supervisorCustomers = await ReadDataAsync<IReadOnlyList<CustomerResponse>>(await supervisor.GetAsync("/api/customers"));

        Assert.All(agentCustomers, c => Assert.Equal(SeedIds.Accounts.NovaBank, c.AccountId));
        Assert.All(supervisorCustomers, c => Assert.Equal(SeedIds.Accounts.NovaBank, c.AccountId));
        Assert.DoesNotContain(agentCustomers, c => c.AccountId == SeedIds.Accounts.Streamly);
    }

    [Fact]
    public async Task CrossScopeCustomer_ReturnsNotFoundToNonAdmin()
    {
        await using var factory = await CustomerApiFactory.CreateAsync();
        var agent = await CreateAuthenticatedClientAsync(factory, AgentEmail);

        var getResponse = await agent.GetAsync($"/api/customers/{SeedIds.Customers.StreamlyCustomer1}");
        Assert.Equal(HttpStatusCode.NotFound, getResponse.StatusCode);

        var updateResponse = await agent.PutAsJsonAsync($"/api/customers/{SeedIds.Customers.StreamlyCustomer1}", new
        {
            accountId = SeedIds.Accounts.Streamly,
            firstName = "Cross",
            lastName = "Scope",
            email = (string?)null,
            phoneNumber = (string?)null,
            externalReference = (string?)null
        });
        Assert.Equal(HttpStatusCode.NotFound, updateResponse.StatusCode);

        var deactivateResponse = await agent.PostAsync($"/api/customers/{SeedIds.Customers.StreamlyCustomer1}/deactivate", null);
        Assert.Equal(HttpStatusCode.NotFound, deactivateResponse.StatusCode);
    }

    [Fact]
    public async Task CustomerTickets_ReturnsSeededDemoHistory()
    {
        await using var factory = await CustomerApiFactory.CreateAsync();
        var client = await CreateAuthenticatedClientAsync(factory, AdminEmail);

        var response = await client.GetAsync($"/api/customers/{SeedIds.Customers.NovaBankCustomer1}/tickets");
        var tickets = await ReadDataAsync<IReadOnlyList<CustomerTicketSummaryResponse>>(response);

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        Assert.Equal(6, tickets.Count);
        Assert.Contains(tickets, ticket => ticket.TicketNumber == "OPS-000001" && ticket.Status == "Open");
        Assert.Contains(tickets, ticket => ticket.TicketNumber == "OPS-000006" && ticket.Status == "Closed");
    }

    [Fact]
    public async Task CustomerAuditLogs_DoNotContainPii()
    {
        await using var factory = await CustomerApiFactory.CreateAsync();
        var client = await CreateAuthenticatedClientAsync(factory, AdminEmail);

        var customer = await CreateCustomerAsync(client, SeedIds.Accounts.NovaBank, "AuditTest", "PiiCheck", "pii.test@fictional.test", "+1-555-000-0001", "AUDIT-001");
        await factory.AssertAuditDoesNotContainPiiAsync(customer.Id);
    }

    [Fact]
    public async Task CustomerResponses_UseEnvelopeAndDoNotExposeSecrets()
    {
        await using var factory = await CustomerApiFactory.CreateAsync();
        var client = await CreateAuthenticatedClientAsync(factory, AdminEmail);

        var response = await client.GetAsync("/api/customers");
        var json = await response.Content.ReadAsStringAsync();

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        Assert.Contains("\"data\"", json, StringComparison.OrdinalIgnoreCase);
        await AssertResponseDoesNotExposeSensitiveDataAsync(response);
    }

    private static async Task<CustomerResponse> CreateCustomerAsync(HttpClient client, Guid accountId, string firstName, string lastName, string? email, string? phoneNumber, string? externalReference)
    {
        var response = await client.PostAsJsonAsync("/api/customers", new { accountId, firstName, lastName, email, phoneNumber, externalReference });
        Assert.Equal(HttpStatusCode.Created, response.StatusCode);
        return await ReadDataAsync<CustomerResponse>(response);
    }

    private static async Task AssertValidationAsync(HttpResponseMessage response, string field)
    {
        var error = await ReadErrorAsync(response);
        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        Assert.Equal("validation_error", error.Error.Code);
        Assert.Contains(error.Error.Details, detail => detail.Field == field);
    }

    private static async Task<HttpClient> CreateAuthenticatedClientAsync(CustomerApiFactory factory, string email)
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

    private static async Task AssertResponseDoesNotExposeSensitiveDataAsync(HttpResponseMessage response)
    {
        var json = await response.Content.ReadAsStringAsync();
        foreach (var term in new[] { "passwordHash", "temporaryPassword", "accessToken", "refreshToken", "jwt", "secret", CustomerApiFactory.JwtSigningKey })
        {
            Assert.DoesNotContain(term, json, StringComparison.OrdinalIgnoreCase);
        }
    }

    private sealed record ApiResponse<T>(T Data);
    private sealed record ApiErrorResponse(ApiError Error);
    private sealed record ApiError(string Code, string Message, IReadOnlyList<ApiErrorDetail> Details);
    private sealed record ApiErrorDetail(string? Field, string Message);
    private sealed record LoginData(string AccessToken);
    private sealed record CustomerResponse(Guid Id, Guid AccountId, string AccountCode, string AccountName, string FirstName, string LastName, string? Email, string? PhoneNumber, string? ExternalReference, bool IsActive);
    private sealed record CustomerTicketSummaryResponse(Guid Id, string TicketNumber, string Status, string Priority, string SlaState, DateTime CreatedAt, DateTime? ResolvedAt, DateTime? ClosedAt);

    internal sealed class CustomerApiFactory : WebApplicationFactory<Program>
    {
        public const string JwtSigningKey = "integration-testing-only-fictional-jwt-signing-key";

        private readonly SqliteConnection connection = new("Data Source=:memory:");

        public static async Task<CustomerApiFactory> CreateAsync()
        {
            ConfigureEnvironment();
            var factory = new CustomerApiFactory();
            await factory.connection.OpenAsync();
            await factory.InitializeDatabaseAsync();
            return factory;
        }

        public async Task<Guid> AddInactiveAccountAsync(string code, string name)
        {
            using var scope = Services.CreateScope();
            var dbContext = scope.ServiceProvider.GetRequiredService<OpsSphereDbContext>();
            var mexicoId = await dbContext.Countries.Where(c => c.Code == "MX").Select(c => c.Id).SingleAsync();
            var account = new Account { Id = Guid.NewGuid(), CountryId = mexicoId, Code = code, Name = name, IsActive = false, CreatedAt = DateTime.UtcNow };
            dbContext.Accounts.Add(account);
            await dbContext.SaveChangesAsync();
            return account.Id;
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

        public async Task AssertAuditDoesNotContainPiiAsync(Guid customerId)
        {
            using var scope = Services.CreateScope();
            var dbContext = scope.ServiceProvider.GetRequiredService<OpsSphereDbContext>();
            var customer = await dbContext.Customers.AsNoTracking().FirstAsync(c => c.Id == customerId);

            var auditLogs = await dbContext.AuditLogs.AsNoTracking()
                .Where(log => log.EntityId == customerId)
                .ToListAsync();

            foreach (var log in auditLogs)
            {
                var combined = $"{log.PreviousValue}{log.NewValue}";
                if (customer.Email is not null) Assert.DoesNotContain(customer.Email, combined, StringComparison.OrdinalIgnoreCase);
                if (customer.PhoneNumber is not null) Assert.DoesNotContain(customer.PhoneNumber, combined, StringComparison.OrdinalIgnoreCase);
                Assert.DoesNotContain(customer.FirstName, combined, StringComparison.OrdinalIgnoreCase);
                Assert.DoesNotContain(customer.LastName, combined, StringComparison.OrdinalIgnoreCase);
            }
        }

        protected override void ConfigureWebHost(IWebHostBuilder builder)
        {
            builder.UseEnvironment("Testing");
            builder.ConfigureAppConfiguration((_, config) =>
            {
                config.AddInMemoryCollection(new Dictionary<string, string?>
                {
                    ["ConnectionStrings:DefaultConnection"] = "Server=(local);Database=OpsSphereCustomerTests;Trusted_Connection=True;TrustServerCertificate=True;",
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
            Environment.SetEnvironmentVariable("ConnectionStrings__DefaultConnection", "Server=(local);Database=OpsSphereCustomerTests;Trusted_Connection=True;TrustServerCertificate=True;");
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
