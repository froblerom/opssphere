using System.IdentityModel.Tokens.Jwt;
using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text.Json;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using OpsSphere.Domain.Entities;
using OpsSphere.Infrastructure.Persistence;
using OpsSphere.Infrastructure.Persistence.SeedData;
using OpsSphere.IntegrationTests.TestInfrastructure;

namespace OpsSphere.IntegrationTests.Auth;

public sealed class AuthApiTests
{
    private const string AgentEmail = "agent.novabank@opssphere.local";
    private const string GenericLoginError = "Invalid email or password.";

    private static readonly JsonSerializerOptions JsonOptions = new(JsonSerializerDefaults.Web);

    [Fact]
    public async Task Login_WithValidSeededUser_ReturnsJwt()
    {
        await using var factory = await OpsSphereSqliteFactory.CreateAsync();
        var client = factory.CreateClient();

        var result = await LoginAsync(client, AgentEmail, OpsSphereSeedData.LocalDemoPassword);

        Assert.Equal(HttpStatusCode.OK, result.Response.StatusCode);
        Assert.False(string.IsNullOrWhiteSpace(result.Body.Data.AccessToken));
        Assert.Equal("Bearer", result.Body.Data.TokenType);
        Assert.Equal(3600, result.Body.Data.ExpiresIn);
        Assert.Equal(AgentEmail, result.Body.Data.User.Email);
        Assert.Contains("Agent", result.Body.Data.User.Roles);
    }

    [Fact]
    public async Task Login_WithInvalidPassword_ReturnsGenericError()
    {
        await using var factory = await OpsSphereSqliteFactory.CreateAsync();
        var client = factory.CreateClient();

        var response = await client.PostAsJsonAsync("/api/auth/login", new
        {
            email = AgentEmail,
            password = "wrong-password"
        });

        var error = await OpsSphereSqliteFactory.ReadResponseAsync<OpsSphereSqliteFactory.ApiErrorResponse>(response);

        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
        Assert.Equal(GenericLoginError, error.Error.Message);
    }

    [Fact]
    public async Task Login_WithUnknownEmail_ReturnsGenericError()
    {
        await using var factory = await OpsSphereSqliteFactory.CreateAsync();
        var client = factory.CreateClient();

        var response = await client.PostAsJsonAsync("/api/auth/login", new
        {
            email = "unknown@opssphere.local",
            password = OpsSphereSeedData.LocalDemoPassword
        });

        var error = await OpsSphereSqliteFactory.ReadResponseAsync<OpsSphereSqliteFactory.ApiErrorResponse>(response);

        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
        Assert.Equal(GenericLoginError, error.Error.Message);
    }

    [Fact]
    public async Task Login_WithInactiveUser_ReturnsGenericError()
    {
        await using var factory = await OpsSphereSqliteFactory.CreateAsync();
        await DeactivateUserAsync(factory, AgentEmail);
        var client = factory.CreateClient();


        var response = await client.PostAsJsonAsync("/api/auth/login", new
        {
            email = AgentEmail,
            password = OpsSphereSeedData.LocalDemoPassword
        });

        var error = await OpsSphereSqliteFactory.ReadResponseAsync<OpsSphereSqliteFactory.ApiErrorResponse>(response);

        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
        Assert.Equal(GenericLoginError, error.Error.Message);
    }

    [Fact]
    public async Task Login_DoesNotRevealInactiveOrMissingUserReason()
    {
        await using var inactiveFactory = await OpsSphereSqliteFactory.CreateAsync();
        await inactiveFactory.DeactivateUserAsync(AgentEmail);
        var inactiveResponse = await inactiveFactory.CreateClient().PostAsJsonAsync("/api/auth/login", new
        {
            email = AgentEmail,
            password = OpsSphereSeedData.LocalDemoPassword
        });
        var inactiveBody = await inactiveResponse.Content.ReadAsStringAsync();

        await using var missingFactory = await OpsSphereSqliteFactory.CreateAsync();
        var missingResponse = await missingFactory.CreateClient().PostAsJsonAsync("/api/auth/login", new
        {
            email = "missing@opssphere.local",
            password = OpsSphereSeedData.LocalDemoPassword
        });
        var missingBody = await missingResponse.Content.ReadAsStringAsync();

        Assert.Equal(HttpStatusCode.Unauthorized, inactiveResponse.StatusCode);
        Assert.Equal(HttpStatusCode.Unauthorized, missingResponse.StatusCode);

        // Compare error code and message only � correlationId differs per-request by design
        var inactiveError = JsonSerializer.Deserialize<OpsSphereSqliteFactory.ApiErrorResponse>(inactiveBody, JsonOptions)!;
        var missingError = JsonSerializer.Deserialize<OpsSphereSqliteFactory.ApiErrorResponse>(missingBody, JsonOptions)!;
        Assert.Equal(inactiveError.Error.Code, missingError.Error.Code);
        Assert.Equal(inactiveError.Error.Message, missingError.Error.Message);
    }

    [Fact]
    public async Task GetMe_WithoutToken_Returns401()
    {
        await using var factory = await OpsSphereSqliteFactory.CreateAsync();

        var response = await factory.CreateClient().GetAsync("/api/auth/me");

        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    [Fact]
    public async Task GetMe_WithValidToken_ReturnsCurrentUserProfile()
    {
        await using var factory = await OpsSphereSqliteFactory.CreateAsync();
        var client = factory.CreateClient();
        var login = await LoginAsync(client, AgentEmail, OpsSphereSeedData.LocalDemoPassword);

        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", login.Body.Data.AccessToken);
        var response = await client.GetAsync("/api/auth/me");
        var body = await OpsSphereSqliteFactory.ReadResponseAsync<OpsSphereSqliteFactory.ApiResponse<CurrentUserProfile>>(response);

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        Assert.Equal(AgentEmail, body.Data.Email);
        Assert.Contains("Agent", body.Data.Roles);
        Assert.Contains("tickets.view", body.Data.Permissions);
        Assert.Contains(body.Data.Scopes, scope => scope.ScopeType == "Campaign" && scope.CampaignCode == "NOVABANK-CC");
    }

    [Fact]
    public async Task ProtectedSmoke_WithoutToken_Returns401()
    {
        await using var factory = await OpsSphereSqliteFactory.CreateAsync();

        var response = await factory.CreateClient().GetAsync("/api/auth/protected-smoke");

        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    [Fact]
    public async Task ProtectedSmoke_WithValidToken_Returns200()
    {
        await using var factory = await OpsSphereSqliteFactory.CreateAsync();
        var client = factory.CreateClient();
        var login = await LoginAsync(client, AgentEmail, OpsSphereSeedData.LocalDemoPassword);

        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", login.Body.Data.AccessToken);
        var response = await client.GetAsync("/api/auth/protected-smoke");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
    }

    [Fact]
    public async Task JwtPayload_DoesNotContainSensitiveData()
    {
        await using var factory = await OpsSphereSqliteFactory.CreateAsync();
        var client = factory.CreateClient();
        var login = await LoginAsync(client, AgentEmail, OpsSphereSeedData.LocalDemoPassword);

        var token = new JwtSecurityTokenHandler().ReadJwtToken(login.Body.Data.AccessToken);

        Assert.Contains(token.Claims, claim => claim.Type == JwtRegisteredClaimNames.Sub);
        Assert.Contains(token.Claims, claim => claim.Type == JwtRegisteredClaimNames.Email && claim.Value == AgentEmail);
        Assert.Contains(token.Claims, claim => claim.Type == "display_name");
        Assert.DoesNotContain(token.Claims, claim => claim.Type.Contains("password", StringComparison.OrdinalIgnoreCase));
        Assert.DoesNotContain(token.Claims, claim => claim.Value.Contains("password", StringComparison.OrdinalIgnoreCase));
        Assert.DoesNotContain(token.Claims, claim => claim.Value.Contains("tickets.view", StringComparison.OrdinalIgnoreCase));
        Assert.DoesNotContain(token.Claims, claim => claim.Type.Contains("permission", StringComparison.OrdinalIgnoreCase));
        Assert.DoesNotContain(token.Claims, claim => claim.Type.Contains("scope", StringComparison.OrdinalIgnoreCase));
        Assert.DoesNotContain(OpsSphereSqliteFactory.JwtSigningKey, login.Body.Data.AccessToken, StringComparison.Ordinal);
    }

    [Fact]
    public async Task Auth_DoesNotReturnPasswordHash()
    {
        await using var factory = await OpsSphereSqliteFactory.CreateAsync();
        var passwordHash = await GetPasswordHashAsync(factory, AgentEmail);
        var client = factory.CreateClient();
        var login = await LoginAsync(client, AgentEmail, OpsSphereSeedData.LocalDemoPassword);
        var loginJson = await login.Response.Content.ReadAsStringAsync();

        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", login.Body.Data.AccessToken);
        var meResponse = await client.GetAsync("/api/auth/me");
        var meJson = await meResponse.Content.ReadAsStringAsync();

        Assert.DoesNotContain("passwordHash", loginJson, StringComparison.OrdinalIgnoreCase);
        Assert.DoesNotContain("passwordHash", meJson, StringComparison.OrdinalIgnoreCase);
        Assert.DoesNotContain(passwordHash, loginJson, StringComparison.Ordinal);
        Assert.DoesNotContain(passwordHash, meJson, StringComparison.Ordinal);
    }

    private static async Task<LoginApiResult> LoginAsync(HttpClient client, string email, string password)
    {
        var response = await client.PostAsJsonAsync("/api/auth/login", new { email, password });
        var body = await OpsSphereSqliteFactory.ReadResponseAsync<OpsSphereSqliteFactory.ApiResponse<LoginResponse>>(response);

        return new LoginApiResult(response, body);
    }

    private sealed record LoginApiResult(HttpResponseMessage Response, OpsSphereSqliteFactory.ApiResponse<LoginResponse> Body);
    private sealed record LoginResponse(string AccessToken, string TokenType, int ExpiresIn, AuthUserSummary User);
    private sealed record AuthUserSummary(Guid Id, string Email, string DisplayName, IReadOnlyList<string> Roles);
    private sealed record CurrentUserProfile(
        Guid Id,
        string Email,
        string DisplayName,
        IReadOnlyList<string> Roles,
        IReadOnlyList<string> Permissions,
        IReadOnlyList<UserScopeResponse> Scopes);
    private sealed record UserScopeResponse(
        string ScopeType,
        Guid? RegionId,
        string? RegionCode,
        Guid? CountryId,
        string? CountryCode,
        Guid? AccountId,
        string? AccountCode,
        Guid? CampaignId,
        string? CampaignCode);

    private static async Task DeactivateUserAsync(OpsSphereSqliteFactory factory, string email)
    {
        using var scope = factory.Services.CreateScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<OpsSphereDbContext>();
        var user = await dbContext.Users.SingleAsync(u => u.Email == email);
        user.IsActive = false;
        user.DeactivatedAt = DateTime.UtcNow;
        await dbContext.SaveChangesAsync();
    }

    private static async Task<string> GetPasswordHashAsync(OpsSphereSqliteFactory factory, string email)
    {
        using var scope = factory.Services.CreateScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<OpsSphereDbContext>();
        var user = await dbContext.Users.AsNoTracking().SingleAsync(u => u.Email == email);
        return user.PasswordHash;
    }
}
