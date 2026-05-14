using System.Net;
using System.Reflection;
using System.Text.Json;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.AspNetCore.TestHost;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using OpsSphere.Application.Common.Exceptions;
using OpsSphere.Infrastructure.Persistence;
using OpsSphere.Infrastructure.Persistence.SeedData;

namespace OpsSphere.IntegrationTests.Middleware;

public sealed class ExceptionHandlingTests
{
    private const string CorrelationIdHeader = "X-Correlation-Id";

    private static readonly JsonSerializerOptions JsonOptions = new(JsonSerializerDefaults.Web);

    // 8. Validation error returns canonical envelope
    [Fact]
    public async Task ValidationException_Returns400_WithCanonicalEnvelope()
    {
        await using var factory = await ExceptionApiFactory.CreateAsync();

        var response = await factory.CreateClient().GetAsync("/api/test-diagnostics/throw-validation");
        var body = await response.Content.ReadAsStringAsync();

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        Assert.Equal("application/json", response.Content.Headers.ContentType?.MediaType);

        var json = JsonDocument.Parse(body).RootElement;
        Assert.True(json.TryGetProperty("error", out var error));
        Assert.Equal("validation_error", error.GetProperty("code").GetString());
        Assert.True(error.TryGetProperty("details", out _));
        Assert.True(error.TryGetProperty("correlationId", out _));
    }

    // 9. Business rule error returns canonical envelope
    [Fact]
    public async Task BusinessRuleException_Returns400_WithCanonicalEnvelope()
    {
        await using var factory = await ExceptionApiFactory.CreateAsync();

        var response = await factory.CreateClient().GetAsync("/api/test-diagnostics/throw-business-rule");
        var body = await response.Content.ReadAsStringAsync();

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);

        var json = JsonDocument.Parse(body).RootElement;
        var error = json.GetProperty("error");
        Assert.Equal("business_rule_violation", error.GetProperty("code").GetString());
        Assert.True(error.TryGetProperty("correlationId", out _));
    }

    // 10. Not found error returns canonical envelope
    [Fact]
    public async Task NotFoundException_Returns404_WithCanonicalEnvelope()
    {
        await using var factory = await ExceptionApiFactory.CreateAsync();

        var response = await factory.CreateClient().GetAsync("/api/test-diagnostics/throw-not-found");
        var body = await response.Content.ReadAsStringAsync();

        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);

        var json = JsonDocument.Parse(body).RootElement;
        var error = json.GetProperty("error");
        Assert.Equal("not_found", error.GetProperty("code").GetString());
        Assert.True(error.TryGetProperty("correlationId", out _));
    }

    // 11. Conflict error returns canonical envelope
    [Fact]
    public async Task ConflictException_Returns409_WithCanonicalEnvelope()
    {
        await using var factory = await ExceptionApiFactory.CreateAsync();

        var response = await factory.CreateClient().GetAsync("/api/test-diagnostics/throw-conflict");
        var body = await response.Content.ReadAsStringAsync();

        Assert.Equal(HttpStatusCode.Conflict, response.StatusCode);

        var json = JsonDocument.Parse(body).RootElement;
        var error = json.GetProperty("error");
        Assert.Equal("conflict", error.GetProperty("code").GetString());
        Assert.True(error.TryGetProperty("correlationId", out _));
    }

    // 12. Unhandled exception returns safe 500 envelope
    [Fact]
    public async Task UnhandledException_Returns500_WithSafeEnvelope()
    {
        await using var factory = await ExceptionApiFactory.CreateAsync();

        var response = await factory.CreateClient().GetAsync("/api/test-diagnostics/throw-unhandled");
        var body = await response.Content.ReadAsStringAsync();

        Assert.Equal(HttpStatusCode.InternalServerError, response.StatusCode);

        var json = JsonDocument.Parse(body).RootElement;
        var error = json.GetProperty("error");
        Assert.Equal("unexpected_error", error.GetProperty("code").GetString());
        Assert.True(error.TryGetProperty("correlationId", out _));
    }

    // 13. Unhandled exception response does not include stack trace
    [Fact]
    public async Task UnhandledException_Response_DoesNotIncludeStackTrace()
    {
        await using var factory = await ExceptionApiFactory.CreateAsync();

        var response = await factory.CreateClient().GetAsync("/api/test-diagnostics/throw-unhandled");
        var body = await response.Content.ReadAsStringAsync();

        Assert.DoesNotContain("StackTrace", body, StringComparison.OrdinalIgnoreCase);
        Assert.DoesNotContain("at OpsSphere", body, StringComparison.Ordinal);
        Assert.DoesNotContain("System.Exception", body, StringComparison.Ordinal);
    }

    // 15. Response does not expose passwords, hashes, tokens, keys, or connection strings
    [Fact]
    public async Task ErrorResponse_DoesNotExposeSecrets()
    {
        await using var factory = await ExceptionApiFactory.CreateAsync();
        var client = factory.CreateClient();

        var unhandledBody = await (await client.GetAsync("/api/test-diagnostics/throw-unhandled")).Content.ReadAsStringAsync();
        var validationBody = await (await client.GetAsync("/api/test-diagnostics/throw-validation")).Content.ReadAsStringAsync();

        foreach (var body in new[] { unhandledBody, validationBody })
        {
            Assert.DoesNotContain(ExceptionApiFactory.JwtSigningKey, body, StringComparison.Ordinal);
            Assert.DoesNotContain("passwordHash", body, StringComparison.OrdinalIgnoreCase);
            Assert.DoesNotContain("password", body, StringComparison.OrdinalIgnoreCase);
        }
    }

    // Error response includes X-Correlation-Id header for all error types
    [Fact]
    public async Task ErrorResponse_AlwaysIncludesCorrelationIdHeader()
    {
        await using var factory = await ExceptionApiFactory.CreateAsync();
        var client = factory.CreateClient();

        var endpoints = new[]
        {
            "/api/test-diagnostics/throw-validation",
            "/api/test-diagnostics/throw-business-rule",
            "/api/test-diagnostics/throw-not-found",
            "/api/test-diagnostics/throw-unhandled"
        };

        foreach (var endpoint in endpoints)
        {
            var response = await client.GetAsync(endpoint);
            Assert.True(response.Headers.Contains(CorrelationIdHeader),
                $"Expected X-Correlation-Id header for {endpoint}");
        }
    }

    internal sealed class ExceptionApiFactory : WebApplicationFactory<Program>
    {
        public const string JwtSigningKey = "integration-testing-only-fictional-jwt-signing-key";

        private readonly SqliteConnection connection = new("Data Source=:memory:");

        public static async Task<ExceptionApiFactory> CreateAsync()
        {
            ConfigureEnvironment();
            var factory = new ExceptionApiFactory();
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
                    ["ConnectionStrings:DefaultConnection"] = "Server=(local);Database=OpsSphereExceptionTests;Trusted_Connection=True;TrustServerCertificate=True;",
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

                // Register the test diagnostics controller from this assembly
                services.AddControllers()
                    .AddApplicationPart(Assembly.GetExecutingAssembly());
            });
        }

        private static void ConfigureEnvironment()
        {
            Environment.SetEnvironmentVariable("ConnectionStrings__DefaultConnection",
                "Server=(local);Database=OpsSphereExceptionTests;Trusted_Connection=True;TrustServerCertificate=True;");
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

/// <summary>
/// Test-only controller that throws specific exceptions to validate global exception handling.
/// Registered only in the ExceptionApiFactory (Testing environment).
/// </summary>
[ApiController]
[Route("api/test-diagnostics")]
public sealed class TestDiagnosticsController : ControllerBase
{
    [HttpGet("throw-validation")]
    public IActionResult ThrowValidation() =>
        throw new ValidationException("email", "Email is required.");

    [HttpGet("throw-business-rule")]
    public IActionResult ThrowBusinessRule() =>
        throw new BusinessRuleException("Ticket must be resolved before it can be closed.");

    [HttpGet("throw-not-found")]
    public IActionResult ThrowNotFound() =>
        throw new NotFoundException("Ticket", Guid.NewGuid());

    [HttpGet("throw-conflict")]
    public IActionResult ThrowConflict() =>
        throw new ConflictException("A user with this email already exists.");

    [HttpGet("throw-unhandled")]
    public IActionResult ThrowUnhandled() =>
        throw new InvalidOperationException("Simulated unhandled exception for testing.");
}
