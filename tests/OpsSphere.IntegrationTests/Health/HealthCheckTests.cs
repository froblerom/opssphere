using System.Net;
using System.Text.Json;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using OpsSphere.Infrastructure.Persistence;
using OpsSphere.Infrastructure.Persistence.SeedData;
using OpsSphere.IntegrationTests.TestInfrastructure;

namespace OpsSphere.IntegrationTests.Health;

public sealed class HealthCheckTests
{
    private static readonly JsonSerializerOptions JsonOptions = new(JsonSerializerDefaults.Web);

    // 1. GET /health returns 200 and safe response shape
    [Fact]
    public async Task Health_Returns200_WithSafeShape()
    {
        await using var factory = await OpsSphereSqliteFactory.CreateAsync();

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
        await using var factory = await OpsSphereSqliteFactory.CreateAsync();

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
        await using var factory = await OpsSphereSqliteFactory.CreateAsync();

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
        await using var factory = await OpsSphereSqliteFactory.CreateAsync();
        var client = factory.CreateClient();

        var healthBody = await (await client.GetAsync("/health")).Content.ReadAsStringAsync();
        var detailsBody = await (await client.GetAsync("/health/details")).Content.ReadAsStringAsync();

        // No secrets or sensitive data
        Assert.DoesNotContain(OpsSphereSqliteFactory.JwtSigningKey, healthBody, StringComparison.Ordinal);
        Assert.DoesNotContain(OpsSphereSqliteFactory.JwtSigningKey, detailsBody, StringComparison.Ordinal);
        Assert.DoesNotContain("password", healthBody, StringComparison.OrdinalIgnoreCase);
        Assert.DoesNotContain("password", detailsBody, StringComparison.OrdinalIgnoreCase);
        Assert.DoesNotContain("Server=", detailsBody, StringComparison.OrdinalIgnoreCase);
    }

    // 5. X-Correlation-Id header is always present in health responses
    [Fact]
    public async Task Health_AlwaysReturnsCorrelationIdHeader()
    {
        await using var factory = await OpsSphereSqliteFactory.CreateAsync();
        var client = factory.CreateClient();

        var healthResponse = await client.GetAsync("/health");
        var detailsResponse = await client.GetAsync("/health/details");

        Assert.True(healthResponse.Headers.Contains("X-Correlation-Id"));
        Assert.True(detailsResponse.Headers.Contains("X-Correlation-Id"));
    }
}