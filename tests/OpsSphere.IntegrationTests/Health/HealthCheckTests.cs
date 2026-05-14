using System.Net;
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

namespace OpsSphere.IntegrationTests.Health;

public sealed class HealthCheckTests
{
    private static readonly JsonSerializerOptions JsonOptions = new(JsonSerializerDefaults.Web);

    // 1. GET /health returns 200 and safe response shape
    [Fact]
    public async Task Health_Returns200_WithSafeShape()
    {
        await using var factory = await HealthApiFactory.CreateAsync();

        var response = await factory.CreateClient().GetAsync("/health");
        var body = await response.Content.ReadAsStringAsync();

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        Assert.Equal("application/json", response.Content.Headers.ContentType?.MediaType);

        var json = JsonDocument.Parse(body).RootElement;
        Assert.Equal("Healthy", json.GetProperty("status").GetString());
        Assert.True(json.TryGetProperty("timestampUtc", out _));
        Assert.True(json.TryGetProperty("correlationId", out _));
    }

    // 2. GET /health/details returns safe detailed shape
    [Fact]
    public async Task HealthDetails_Returns200_WithSafeShape()
    {
        await using var factory = await HealthApiFactory.CreateAsync();

        var response = await factory.CreateClient().GetAsync("/health/details");
        var body = await response.Content.ReadAsStringAsync();

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);

        var json = JsonDocument.Parse(body).RootElement;
        Assert.Equal("Healthy", json.GetProperty("status").GetString());
        Assert.True(json.TryGetProperty("timestampUtc", out _));
        Assert.True(json.TryGetProperty("correlationId", out _));
        Assert.True(json.TryGetProperty("checks", out _));
    }

    // 3. Database health check is included
    [Fact]
    public async Task HealthDetails_IncludesDatabaseCheck()
    {
        await using var factory = await HealthApiFactory.CreateAsync();

        var response = await factory.CreateClient().GetAsync("/health/details");
        var body = await response.Content.ReadAsStringAsync();
        var json = JsonDocument.Parse(body).RootElement;

        var checks = json.GetProperty("checks").EnumerateArray().ToList();
        Assert.Contains(checks, c => c.GetProperty("name").GetString() == "database");
    }

    // 4. Health response does not expose secrets or raw exception details
    [Fact]
    public async Task Health_DoesNotExposeSecrets()
    {
        await using var factory = await HealthApiFactory.CreateAsync();
        var client = factory.CreateClient();

        var healthBody = await (await client.GetAsync("/health")).Content.ReadAsStringAsync();
        var detailsBody = await (await client.GetAsync("/health/details")).Content.ReadAsStringAsync();

        // No secrets or sensitive data
        Assert.DoesNotContain(HealthApiFactory.JwtSigningKey, healthBody, StringComparison.Ordinal);
        Assert.DoesNotContain(HealthApiFactory.JwtSigningKey, detailsBody, StringComparison.Ordinal);
        Assert.DoesNotContain("password", healthBody, StringComparison.OrdinalIgnoreCase);
        Assert.DoesNotContain("password", detailsBody, StringComparison.OrdinalIgnoreCase);
        Assert.DoesNotContain("Server=", detailsBody, StringComparison.OrdinalIgnoreCase);
    }

    // 5. X-Correlation-Id header is always present in health responses
    [Fact]
    public async Task Health_AlwaysReturnsCorrelationIdHeader()
    {
        await using var factory = await HealthApiFactory.CreateAsync();
        var client = factory.CreateClient();

        var healthResponse = await client.GetAsync("/health");
        var detailsResponse = await client.GetAsync("/health/details");

        Assert.True(healthResponse.Headers.Contains("X-Correlation-Id"));
        Assert.True(detailsResponse.Headers.Contains("X-Correlation-Id"));
    }

    internal sealed class HealthApiFactory : WebApplicationFactory<Program>
    {
        public const string JwtSigningKey = "integration-testing-only-fictional-jwt-signing-key";

        private readonly SqliteConnection connection = new("Data Source=:memory:");

        public static async Task<HealthApiFactory> CreateAsync()
        {
            ConfigureEnvironment();
            var factory = new HealthApiFactory();
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
                    ["ConnectionStrings:DefaultConnection"] = "Server=(local);Database=OpsSphereHealthTests;Trusted_Connection=True;TrustServerCertificate=True;",
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
                "Server=(local);Database=OpsSphereHealthTests;Trusted_Connection=True;TrustServerCertificate=True;");
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
