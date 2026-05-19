using System.Net;
using System.Reflection;
using System.Text.Json;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using OpsSphere.Application.Common.Exceptions;
using OpsSphere.Infrastructure.Persistence;
using OpsSphere.Infrastructure.Persistence.SeedData;
using OpsSphere.IntegrationTests.TestInfrastructure;

namespace OpsSphere.IntegrationTests.Middleware;

public sealed class ExceptionHandlingTests
{
    private const string CorrelationIdHeader = "X-Correlation-Id";

    private static readonly JsonSerializerOptions JsonOptions = new(JsonSerializerDefaults.Web);

    // 8. Validation error returns canonical envelope
    [Fact]
    public async Task ValidationException_Returns400_WithCanonicalEnvelope()
    {
        await using var factory = await OpsSphereSqliteFactory.CreateAsync();

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
        await using var factory = await OpsSphereSqliteFactory.CreateAsync();

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
        await using var factory = await OpsSphereSqliteFactory.CreateAsync();

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
        await using var factory = await OpsSphereSqliteFactory.CreateAsync();

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
        await using var factory = await OpsSphereSqliteFactory.CreateAsync();

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
        await using var factory = await OpsSphereSqliteFactory.CreateAsync();

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
        await using var factory = await OpsSphereSqliteFactory.CreateAsync();
        var client = factory.CreateClient();

        var unhandledBody = await (await client.GetAsync("/api/test-diagnostics/throw-unhandled")).Content.ReadAsStringAsync();
        var validationBody = await (await client.GetAsync("/api/test-diagnostics/throw-validation")).Content.ReadAsStringAsync();

        foreach (var body in new[] { unhandledBody, validationBody })
        {
            Assert.DoesNotContain(OpsSphereSqliteFactory.JwtSigningKey, body, StringComparison.Ordinal);
            Assert.DoesNotContain("passwordHash", body, StringComparison.OrdinalIgnoreCase);
            Assert.DoesNotContain("password", body, StringComparison.OrdinalIgnoreCase);
        }
    }

    // Error response includes X-Correlation-Id header for all error types
    [Fact]
    public async Task ErrorResponse_AlwaysIncludesCorrelationIdHeader()
    {
        await using var factory = await OpsSphereSqliteFactory.CreateAsync();
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
}