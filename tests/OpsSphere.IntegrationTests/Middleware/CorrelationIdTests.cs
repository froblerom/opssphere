using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using OpsSphere.Infrastructure.Persistence;
using OpsSphere.Infrastructure.Persistence.SeedData;
using OpsSphere.IntegrationTests.TestInfrastructure;

namespace OpsSphere.IntegrationTests.Middleware;

public sealed class CorrelationIdTests
{
    private const string CorrelationIdHeader = "X-Correlation-Id";

    private static readonly JsonSerializerOptions JsonOptions = new(JsonSerializerDefaults.Web);

    // 4. Missing X-Correlation-Id creates a new response header
    [Fact]
    public async Task Request_WithoutCorrelationId_GeneratesNewHeader()
    {
        await using var factory = await OpsSphereSqliteFactory.CreateAsync();
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
        await using var factory = await OpsSphereSqliteFactory.CreateAsync();
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
        await using var factory = await OpsSphereSqliteFactory.CreateAsync();
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
        await using var factory = await OpsSphereSqliteFactory.CreateAsync();
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
        await using var factory = await OpsSphereSqliteFactory.CreateAsync();
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
        await using var factory = await OpsSphereSqliteFactory.CreateAsync();

        var response = await factory.CreateClient().GetAsync("/api/auth/me");

        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
        Assert.True(response.Headers.Contains(CorrelationIdHeader));
    }
}