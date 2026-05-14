using System.Net;
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

namespace OpsSphere.IntegrationTests.Middleware;

public sealed class CorrelationIdTests
{
    private const string CorrelationIdHeader = "X-Correlation-Id";

    private static readonly JsonSerializerOptions JsonOptions = new(JsonSerializerDefaults.Web);

    // 4. Missing X-Correlation-Id creates a new response header
    [Fact]
    public async Task Request_WithoutCorrelationId_GeneratesNewHeader()
    {
        await using var factory = await CorrelationApiFactory.CreateAsync();
        var client = factory.CreateClient();

        var response = await client.GetAsync("/health");

        Assert.True(response.Headers.Contains(CorrelationIdHeader));
        var value = response.Headers.GetValues(CorrelationIdHeader).First();
        Assert.False(string.IsNullOrWhiteSpace(value));
    }

    // 5. Incoming safe X-Correlation-Id is reused in response header
    [Fact]
    public async Task Request_WithSafeCorrelationId_ReusesItInResponse()
    {
        await using var factory = await CorrelationApiFactory.CreateAsync();
        var client = factory.CreateClient();
        client.DefaultRequestHeaders.Add(CorrelationIdHeader, "test-correlation-123");

        var response = await client.GetAsync("/health");

        Assert.True(response.Headers.Contains(CorrelationIdHeader));
        var value = response.Headers.GetValues(CorrelationIdHeader).First();
        Assert.Equal("test-correlation-123", value);
    }

    // 6. Unsafe incoming X-Correlation-Id is replaced with a generated safe value
    [Theory]
    [InlineData("injected\r\nX-Custom-Header: value")]   // CRLF injection
    [InlineData("<script>alert('xss')</script>")]         // angle brackets
    [InlineData("id with spaces")]                        // spaces
    [InlineData("a\"quoted\"id")]                         // quotes
    [InlineData("")]                                      // empty
    public async Task Request_WithUnsafeCorrelationId_GeneratesNewSafeHeader(string unsafeId)
    {
        await using var factory = await CorrelationApiFactory.CreateAsync();
        var client = factory.CreateClient();

        if (!string.IsNullOrEmpty(unsafeId))
        {
            client.DefaultRequestHeaders.TryAddWithoutValidation(CorrelationIdHeader, unsafeId);
        }

        var response = await client.GetAsync("/health");

        Assert.True(response.Headers.Contains(CorrelationIdHeader));
        var value = response.Headers.GetValues(CorrelationIdHeader).First();

        // Generated value must not contain injected content
        Assert.DoesNotContain("\r", value, StringComparison.Ordinal);
        Assert.DoesNotContain("\n", value, StringComparison.Ordinal);
        Assert.DoesNotContain("<", value, StringComparison.Ordinal);
        Assert.False(string.IsNullOrWhiteSpace(value));
    }

    // Correlation ID appears in error envelope body
    [Fact]
    public async Task ErrorResponse_IncludesCorrelationId_FromHeader()
    {
        await using var factory = await CorrelationApiFactory.CreateAsync();
        var client = factory.CreateClient();
        client.DefaultRequestHeaders.Add(CorrelationIdHeader, "err-corr-id-abc");

        // Hit a protected endpoint without a token — expect 401 or known error
        // Use a known-failing login to trigger auth error response which now includes correlationId
        var response = await client.PostAsJsonAsync("/api/auth/login", new { email = "x@x.com", password = "bad" });
        var body = await response.Content.ReadAsStringAsync();

        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
        // The X-Correlation-Id response header must match what we sent
        var headerValue = response.Headers.GetValues(CorrelationIdHeader).First();
        Assert.Equal("err-corr-id-abc", headerValue);
        // The response body (canonical envelope) must include the correlationId
        Assert.Contains("err-corr-id-abc", body, StringComparison.Ordinal);
    }

    // Correlation ID propagates to health endpoint body
    [Fact]
    public async Task HealthResponse_IncludesCorrelationIdInBody()
    {
        await using var factory = await CorrelationApiFactory.CreateAsync();
        var client = factory.CreateClient();
        client.DefaultRequestHeaders.Add(CorrelationIdHeader, "health-corr-abc");

        var response = await client.GetAsync("/health");
        var body = await response.Content.ReadAsStringAsync();

        var json = JsonDocument.Parse(body).RootElement;
        Assert.Equal("health-corr-abc", json.GetProperty("correlationId").GetString());
    }

    // 16. 401/403 responses remain consistent and include X-Correlation-Id header
    [Fact]
    public async Task Unauthenticated_Request_Returns401_WithCorrelationIdHeader()
    {
        await using var factory = await CorrelationApiFactory.CreateAsync();

        var response = await factory.CreateClient().GetAsync("/api/auth/me");

        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
        Assert.True(response.Headers.Contains(CorrelationIdHeader));
    }

    internal sealed class CorrelationApiFactory : WebApplicationFactory<Program>
    {
        public const string JwtSigningKey = "integration-testing-only-fictional-jwt-signing-key";

        private readonly SqliteConnection connection = new("Data Source=:memory:");

        public static async Task<CorrelationApiFactory> CreateAsync()
        {
            ConfigureEnvironment();
            var factory = new CorrelationApiFactory();
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
                    ["ConnectionStrings:DefaultConnection"] = "Server=(local);Database=OpsSphereCorrelationTests;Trusted_Connection=True;TrustServerCertificate=True;",
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
            Environment.SetEnvironmentVariable("ConnectionStrings__DefaultConnection",
                "Server=(local);Database=OpsSphereCorrelationTests;Trusted_Connection=True;TrustServerCertificate=True;");
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
