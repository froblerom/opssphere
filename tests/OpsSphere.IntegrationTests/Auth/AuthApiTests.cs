using System.IdentityModel.Tokens.Jwt;
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

namespace OpsSphere.IntegrationTests.Auth;

public sealed class AuthApiTests
{
    private const string AgentEmail = "agent.novabank@opssphere.local";
    private const string GenericLoginError = "Invalid email or password.";

    private static readonly JsonSerializerOptions JsonOptions = new(JsonSerializerDefaults.Web);

    [Fact]
    public async Task Login_WithValidSeededUser_ReturnsJwt()
    {
        await using var factory = await AuthApiFactory.CreateAsync();
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
        await using var factory = await AuthApiFactory.CreateAsync();
        var client = factory.CreateClient();

        var response = await client.PostAsJsonAsync("/api/auth/login", new
        {
            email = AgentEmail,
            password = "wrong-password"
        });

        var error = await ReadResponseAsync<ApiErrorResponse>(response);

        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
        Assert.Equal(GenericLoginError, error.Error.Message);
    }

    [Fact]
    public async Task Login_WithUnknownEmail_ReturnsGenericError()
    {
        await using var factory = await AuthApiFactory.CreateAsync();
        var client = factory.CreateClient();

        var response = await client.PostAsJsonAsync("/api/auth/login", new
        {
            email = "unknown@opssphere.local",
            password = OpsSphereSeedData.LocalDemoPassword
        });

        var error = await ReadResponseAsync<ApiErrorResponse>(response);

        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
        Assert.Equal(GenericLoginError, error.Error.Message);
    }

    [Fact]
    public async Task Login_WithInactiveUser_ReturnsGenericError()
    {
        await using var factory = await AuthApiFactory.CreateAsync();
        await factory.DeactivateUserAsync(AgentEmail);
        var client = factory.CreateClient();

        var response = await client.PostAsJsonAsync("/api/auth/login", new
        {
            email = AgentEmail,
            password = OpsSphereSeedData.LocalDemoPassword
        });

        var error = await ReadResponseAsync<ApiErrorResponse>(response);

        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
        Assert.Equal(GenericLoginError, error.Error.Message);
    }

    [Fact]
    public async Task Login_DoesNotRevealInactiveOrMissingUserReason()
    {
        await using var inactiveFactory = await AuthApiFactory.CreateAsync();
        await inactiveFactory.DeactivateUserAsync(AgentEmail);
        var inactiveResponse = await inactiveFactory.CreateClient().PostAsJsonAsync("/api/auth/login", new
        {
            email = AgentEmail,
            password = OpsSphereSeedData.LocalDemoPassword
        });
        var inactiveBody = await inactiveResponse.Content.ReadAsStringAsync();

        await using var missingFactory = await AuthApiFactory.CreateAsync();
        var missingResponse = await missingFactory.CreateClient().PostAsJsonAsync("/api/auth/login", new
        {
            email = "missing@opssphere.local",
            password = OpsSphereSeedData.LocalDemoPassword
        });
        var missingBody = await missingResponse.Content.ReadAsStringAsync();

        Assert.Equal(HttpStatusCode.Unauthorized, inactiveResponse.StatusCode);
        Assert.Equal(HttpStatusCode.Unauthorized, missingResponse.StatusCode);
        Assert.Equal(inactiveBody, missingBody);
    }

    [Fact]
    public async Task GetMe_WithoutToken_Returns401()
    {
        await using var factory = await AuthApiFactory.CreateAsync();

        var response = await factory.CreateClient().GetAsync("/api/auth/me");

        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    [Fact]
    public async Task GetMe_WithValidToken_ReturnsCurrentUserProfile()
    {
        await using var factory = await AuthApiFactory.CreateAsync();
        var client = factory.CreateClient();
        var login = await LoginAsync(client, AgentEmail, OpsSphereSeedData.LocalDemoPassword);

        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", login.Body.Data.AccessToken);
        var response = await client.GetAsync("/api/auth/me");
        var body = await ReadResponseAsync<ApiResponse<CurrentUserProfile>>(response);

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        Assert.Equal(AgentEmail, body.Data.Email);
        Assert.Contains("Agent", body.Data.Roles);
        Assert.Contains("tickets.view", body.Data.Permissions);
        Assert.Contains(body.Data.Scopes, scope => scope.ScopeType == "Campaign" && scope.CampaignCode == "NOVABANK-CC");
    }

    [Fact]
    public async Task ProtectedSmoke_WithoutToken_Returns401()
    {
        await using var factory = await AuthApiFactory.CreateAsync();

        var response = await factory.CreateClient().GetAsync("/api/auth/protected-smoke");

        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    [Fact]
    public async Task ProtectedSmoke_WithValidToken_Returns200()
    {
        await using var factory = await AuthApiFactory.CreateAsync();
        var client = factory.CreateClient();
        var login = await LoginAsync(client, AgentEmail, OpsSphereSeedData.LocalDemoPassword);

        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", login.Body.Data.AccessToken);
        var response = await client.GetAsync("/api/auth/protected-smoke");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
    }

    [Fact]
    public async Task JwtPayload_DoesNotContainSensitiveData()
    {
        await using var factory = await AuthApiFactory.CreateAsync();
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
        Assert.DoesNotContain(AuthApiFactory.JwtSigningKey, login.Body.Data.AccessToken, StringComparison.Ordinal);
    }

    [Fact]
    public async Task Auth_DoesNotReturnPasswordHash()
    {
        await using var factory = await AuthApiFactory.CreateAsync();
        var passwordHash = await factory.GetPasswordHashAsync(AgentEmail);
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
        var body = await ReadResponseAsync<ApiResponse<LoginResponse>>(response);

        return new LoginApiResult(response, body);
    }

    private static async Task<T> ReadResponseAsync<T>(HttpResponseMessage response)
    {
        var json = await response.Content.ReadAsStringAsync();
        var value = JsonSerializer.Deserialize<T>(json, JsonOptions);

        return value ?? throw new InvalidOperationException($"Response body could not be deserialized as {typeof(T).Name}.");
    }

    private sealed record LoginApiResult(HttpResponseMessage Response, ApiResponse<LoginResponse> Body);
    private sealed record ApiResponse<T>(T Data);
    private sealed record ApiErrorResponse(ApiError Error);
    private sealed record ApiError(string Code, string Message);
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

    private sealed class AuthApiFactory : WebApplicationFactory<Program>
    {
        public const string JwtSigningKey = "integration-testing-only-fictional-jwt-signing-key";

        private readonly SqliteConnection connection = new("Data Source=:memory:");

        public static async Task<AuthApiFactory> CreateAsync()
        {
            ConfigureEnvironment();
            var factory = new AuthApiFactory();
            await factory.connection.OpenAsync();
            await factory.InitializeDatabaseAsync();

            return factory;
        }

        public async Task DeactivateUserAsync(string email)
        {
            using var scope = Services.CreateScope();
            var dbContext = scope.ServiceProvider.GetRequiredService<OpsSphereDbContext>();
            var user = await dbContext.Users.SingleAsync(u => u.Email == email);
            user.IsActive = false;
            user.DeactivatedAt = DateTime.UtcNow;
            await dbContext.SaveChangesAsync();
        }

        public async Task<string> GetPasswordHashAsync(string email)
        {
            using var scope = Services.CreateScope();
            var dbContext = scope.ServiceProvider.GetRequiredService<OpsSphereDbContext>();
            var user = await dbContext.Users.AsNoTracking().SingleAsync(u => u.Email == email);

            return user.PasswordHash;
        }

        protected override void ConfigureWebHost(IWebHostBuilder builder)
        {
            builder.UseEnvironment("Testing");
            builder.ConfigureAppConfiguration((_, configuration) =>
            {
                configuration.AddInMemoryCollection(new Dictionary<string, string?>
                {
                    ["ConnectionStrings:DefaultConnection"] = "Server=(local);Database=OpsSphereAuthTests;Trusted_Connection=True;TrustServerCertificate=True;",
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
            Environment.SetEnvironmentVariable(
                "ConnectionStrings__DefaultConnection",
                "Server=(local);Database=OpsSphereAuthTests;Trusted_Connection=True;TrustServerCertificate=True;");
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
