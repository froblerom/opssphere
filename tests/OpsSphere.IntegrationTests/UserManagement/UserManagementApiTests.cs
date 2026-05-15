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

namespace OpsSphere.IntegrationTests.UserManagement;

public sealed class UserManagementApiTests
{
    private const string AdminEmail = "admin@opssphere.local";
    private const string AgentEmail = "agent.novabank@opssphere.local";
    private const string ManagedUserEmail = "marisol.vega@opssphere.test";
    private const string ManagedUserUpdatedEmail = "marisol.ortega@opssphere.test";
    private const string TemporaryPassword = "FictionalPass123!";

    private static readonly JsonSerializerOptions JsonOptions = new(JsonSerializerDefaults.Web);

    [Fact]
    public async Task Admin_CanListViewCreateUpdateDeactivateAndAssignRoles()
    {
        await using var factory = await UserManagementApiFactory.CreateAsync();
        var client = await CreateAuthenticatedClientAsync(factory, AdminEmail);

        var listResponse = await client.GetAsync("/api/users");
        var listBody = await ReadResponseAsync<ApiResponse<IReadOnlyList<UserSummaryResponse>>>(listResponse);

        Assert.Equal(HttpStatusCode.OK, listResponse.StatusCode);
        Assert.Contains(listBody.Data, user => user.Email == AdminEmail);
        await AssertResponseDoesNotExposeSensitiveDataAsync(factory, listResponse);

        var createResponse = await client.PostAsJsonAsync("/api/users", new
        {
            email = ManagedUserEmail,
            firstName = "Marisol",
            lastName = "Vega",
            displayName = "Marisol Vega",
            temporaryPassword = TemporaryPassword
        });
        var created = await ReadResponseAsync<ApiResponse<UserDetailResponse>>(createResponse);

        Assert.Equal(HttpStatusCode.Created, createResponse.StatusCode);
        Assert.Equal(ManagedUserEmail, created.Data.Email);
        Assert.True(created.Data.IsActive);
        Assert.Empty(created.Data.Roles);
        await AssertResponseDoesNotExposeSensitiveDataAsync(factory, createResponse, TemporaryPassword);
        await factory.AssertAuditAsync("UserCreated", created.Data.Id);

        var viewResponse = await client.GetAsync($"/api/users/{created.Data.Id}");
        var viewed = await ReadResponseAsync<ApiResponse<UserDetailResponse>>(viewResponse);

        Assert.Equal(HttpStatusCode.OK, viewResponse.StatusCode);
        Assert.Equal(created.Data.Id, viewed.Data.Id);
        Assert.Equal(ManagedUserEmail, viewed.Data.Email);
        await AssertResponseDoesNotExposeSensitiveDataAsync(factory, viewResponse, TemporaryPassword);

        var updateResponse = await client.PutAsJsonAsync($"/api/users/{created.Data.Id}", new
        {
            email = ManagedUserUpdatedEmail,
            firstName = "Marisol",
            lastName = "Ortega",
            displayName = "Marisol Ortega"
        });
        var updated = await ReadResponseAsync<ApiResponse<UserDetailResponse>>(updateResponse);

        Assert.Equal(HttpStatusCode.OK, updateResponse.StatusCode);
        Assert.Equal(ManagedUserUpdatedEmail, updated.Data.Email);
        Assert.Equal("Marisol Ortega", updated.Data.DisplayName);
        await AssertResponseDoesNotExposeSensitiveDataAsync(factory, updateResponse, TemporaryPassword);
        await factory.AssertAuditAsync("UserUpdated", created.Data.Id);

        var assignAgentRoleResponse = await client.PutAsJsonAsync($"/api/users/{created.Data.Id}/roles", new
        {
            roleIds = new[] { SeedIds.Roles.Agent }
        });
        var agentRoleUser = await ReadResponseAsync<ApiResponse<UserDetailResponse>>(assignAgentRoleResponse);

        Assert.Equal(HttpStatusCode.OK, assignAgentRoleResponse.StatusCode);
        Assert.Contains(agentRoleUser.Data.Roles, role => role.Name == "Agent");
        await AssertResponseDoesNotExposeSensitiveDataAsync(factory, assignAgentRoleResponse, TemporaryPassword);
        await factory.AssertAuditAsync("RoleAssigned", created.Data.Id);

        var replaceRoleResponse = await client.PutAsJsonAsync($"/api/users/{created.Data.Id}/roles", new
        {
            roleIds = new[] { SeedIds.Roles.Viewer }
        });
        var viewerRoleUser = await ReadResponseAsync<ApiResponse<UserDetailResponse>>(replaceRoleResponse);

        Assert.Equal(HttpStatusCode.OK, replaceRoleResponse.StatusCode);
        Assert.DoesNotContain(viewerRoleUser.Data.Roles, role => role.Name == "Agent");
        Assert.Contains(viewerRoleUser.Data.Roles, role => role.Name == "Viewer");
        await AssertResponseDoesNotExposeSensitiveDataAsync(factory, replaceRoleResponse, TemporaryPassword);
        await factory.AssertAuditAsync("RoleRemoved", created.Data.Id);

        var deactivateResponse = await client.PostAsync($"/api/users/{created.Data.Id}/deactivate", null);

        Assert.Equal(HttpStatusCode.NoContent, deactivateResponse.StatusCode);
        await factory.AssertAuditAsync("UserDeactivated", created.Data.Id);

        var deactivatedViewResponse = await client.GetAsync($"/api/users/{created.Data.Id}");
        var deactivatedUser = await ReadResponseAsync<ApiResponse<UserDetailResponse>>(deactivatedViewResponse);

        Assert.False(deactivatedUser.Data.IsActive);
        Assert.NotNull(deactivatedUser.Data.DeactivatedAt);
    }

    [Fact]
    public async Task CreateUser_WithDuplicateEmail_Returns409()
    {
        await using var factory = await UserManagementApiFactory.CreateAsync();
        var client = await CreateAuthenticatedClientAsync(factory, AdminEmail);

        var response = await client.PostAsJsonAsync("/api/users", new
        {
            email = AgentEmail,
            firstName = "Tomas",
            lastName = "Ibarra",
            displayName = "Tomas Ibarra",
            temporaryPassword = TemporaryPassword
        });
        var error = await ReadResponseAsync<ApiErrorResponse>(response);

        Assert.Equal(HttpStatusCode.Conflict, response.StatusCode);
        Assert.Equal("conflict", error.Error.Code);
        Assert.DoesNotContain("password", await response.Content.ReadAsStringAsync(), StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public async Task CreateUser_WithMissingRequiredFields_ReturnsValidationErrors()
    {
        await using var factory = await UserManagementApiFactory.CreateAsync();
        var client = await CreateAuthenticatedClientAsync(factory, AdminEmail);

        var response = await client.PostAsJsonAsync("/api/users", new
        {
            email = "",
            firstName = "",
            lastName = "",
            displayName = "",
            temporaryPassword = ""
        });
        var error = await ReadResponseAsync<ApiErrorResponse>(response);

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        Assert.Equal("validation_error", error.Error.Code);
        Assert.Contains(error.Error.Details, detail => detail.Field == "email");
        Assert.Contains(error.Error.Details, detail => detail.Field == "firstName");
        Assert.Contains(error.Error.Details, detail => detail.Field == "lastName");
        Assert.Contains(error.Error.Details, detail => detail.Field == "temporaryPassword");
    }

    [Fact]
    public async Task DeactivatedUser_CannotAuthenticate()
    {
        await using var factory = await UserManagementApiFactory.CreateAsync();
        var adminClient = await CreateAuthenticatedClientAsync(factory, AdminEmail);

        var createResponse = await adminClient.PostAsJsonAsync("/api/users", new
        {
            email = ManagedUserEmail,
            firstName = "Marisol",
            lastName = "Vega",
            displayName = "Marisol Vega",
            temporaryPassword = TemporaryPassword
        });
        var created = await ReadResponseAsync<ApiResponse<UserDetailResponse>>(createResponse);

        var roleResponse = await adminClient.PutAsJsonAsync($"/api/users/{created.Data.Id}/roles", new
        {
            roleIds = new[] { SeedIds.Roles.Viewer }
        });
        roleResponse.EnsureSuccessStatusCode();

        var activeLoginResponse = await factory.CreateClient().PostAsJsonAsync("/api/auth/login", new
        {
            email = ManagedUserEmail,
            password = TemporaryPassword
        });
        Assert.Equal(HttpStatusCode.OK, activeLoginResponse.StatusCode);

        var deactivateResponse = await adminClient.PostAsync($"/api/users/{created.Data.Id}/deactivate", null);
        Assert.Equal(HttpStatusCode.NoContent, deactivateResponse.StatusCode);

        var deactivatedLoginResponse = await factory.CreateClient().PostAsJsonAsync("/api/auth/login", new
        {
            email = ManagedUserEmail,
            password = TemporaryPassword
        });
        var error = await ReadResponseAsync<ApiErrorResponse>(deactivatedLoginResponse);

        Assert.Equal(HttpStatusCode.Unauthorized, deactivatedLoginResponse.StatusCode);
        Assert.Equal("Invalid email or password.", error.Error.Message);
    }

    [Fact]
    public async Task NonAdmin_CannotManageUsers()
    {
        await using var factory = await UserManagementApiFactory.CreateAsync();
        var client = await CreateAuthenticatedClientAsync(factory, AgentEmail);

        var listResponse = await client.GetAsync("/api/users");
        var createResponse = await client.PostAsJsonAsync("/api/users", new
        {
            email = ManagedUserEmail,
            firstName = "Marisol",
            lastName = "Vega",
            displayName = "Marisol Vega",
            temporaryPassword = TemporaryPassword
        });
        var rolesResponse = await client.PutAsJsonAsync($"/api/users/{SeedIds.Users.ViewerLatam}/roles", new
        {
            roleIds = new[] { SeedIds.Roles.Viewer }
        });

        Assert.Equal(HttpStatusCode.Forbidden, listResponse.StatusCode);
        Assert.Equal(HttpStatusCode.Forbidden, createResponse.StatusCode);
        Assert.Equal(HttpStatusCode.Forbidden, rolesResponse.StatusCode);
    }

    [Fact]
    public async Task UserManagementResponses_DoNotExposeSensitiveFields()
    {
        await using var factory = await UserManagementApiFactory.CreateAsync();
        var client = await CreateAuthenticatedClientAsync(factory, AdminEmail);

        var createResponse = await client.PostAsJsonAsync("/api/users", new
        {
            email = ManagedUserEmail,
            firstName = "Marisol",
            lastName = "Vega",
            displayName = "Marisol Vega",
            temporaryPassword = TemporaryPassword
        });
        var created = await ReadResponseAsync<ApiResponse<UserDetailResponse>>(createResponse);

        var responses = new[]
        {
            createResponse,
            await client.GetAsync("/api/users"),
            await client.GetAsync($"/api/users/{created.Data.Id}"),
            await client.PutAsJsonAsync($"/api/users/{created.Data.Id}", new
            {
                email = ManagedUserUpdatedEmail,
                firstName = "Marisol",
                lastName = "Ortega",
                displayName = "Marisol Ortega"
            }),
            await client.GetAsync("/api/roles"),
            await client.GetAsync("/api/permissions")
        };

        foreach (var response in responses)
        {
            Assert.True(response.IsSuccessStatusCode, $"Expected success but received {(int)response.StatusCode}.");
            await AssertResponseDoesNotExposeSensitiveDataAsync(factory, response, TemporaryPassword);
        }
    }

    private static async Task<HttpClient> CreateAuthenticatedClientAsync(UserManagementApiFactory factory, string email)
    {
        var client = factory.CreateClient();
        var loginResponse = await client.PostAsJsonAsync("/api/auth/login", new
        {
            email,
            password = OpsSphereSeedData.LocalDemoPassword
        });
        loginResponse.EnsureSuccessStatusCode();
        var loginBody = await ReadResponseAsync<LoginApiResponse>(loginResponse);
        client.DefaultRequestHeaders.Authorization =
            new AuthenticationHeaderValue("Bearer", loginBody.Data.AccessToken);

        return client;
    }

    private static async Task AssertResponseDoesNotExposeSensitiveDataAsync(
        UserManagementApiFactory factory,
        HttpResponseMessage response,
        string? submittedTemporaryPassword = null)
    {
        var json = await response.Content.ReadAsStringAsync();
        var passwordHashes = await factory.GetPasswordHashesAsync();
        var forbiddenTerms = new[]
        {
            "passwordHash",
            "temporaryPassword",
            "accessToken",
            "refreshToken",
            "jwt",
            "secret",
            UserManagementApiFactory.JwtSigningKey
        };

        foreach (var term in forbiddenTerms)
        {
            Assert.DoesNotContain(term, json, StringComparison.OrdinalIgnoreCase);
        }

        if (submittedTemporaryPassword is not null)
        {
            Assert.DoesNotContain(submittedTemporaryPassword, json, StringComparison.Ordinal);
        }

        foreach (var passwordHash in passwordHashes)
        {
            Assert.DoesNotContain(passwordHash, json, StringComparison.Ordinal);
        }
    }

    private static async Task<T> ReadResponseAsync<T>(HttpResponseMessage response)
    {
        var json = await response.Content.ReadAsStringAsync();
        var value = JsonSerializer.Deserialize<T>(json, JsonOptions);

        return value ?? throw new InvalidOperationException($"Could not deserialize {typeof(T).Name}.");
    }

    private sealed record ApiResponse<T>(T Data);
    private sealed record ApiErrorResponse(ApiError Error);
    private sealed record ApiError(string Code, string Message, IReadOnlyList<ApiErrorDetail> Details);
    private sealed record ApiErrorDetail(string? Field, string Message);
    private sealed record LoginApiResponse(LoginData Data);
    private sealed record LoginData(string AccessToken);
    private sealed record UserSummaryResponse(
        Guid Id,
        string Email,
        string DisplayName,
        bool IsActive,
        IReadOnlyList<RoleResponse> Roles);
    private sealed record UserDetailResponse(
        Guid Id,
        string Email,
        string FirstName,
        string LastName,
        string DisplayName,
        bool IsActive,
        DateTime? DeactivatedAt,
        IReadOnlyList<RoleResponse> Roles);
    private sealed record RoleResponse(Guid Id, string Name);

    internal sealed class UserManagementApiFactory : WebApplicationFactory<Program>
    {
        public const string JwtSigningKey = "integration-testing-only-fictional-jwt-signing-key";

        private readonly SqliteConnection connection = new("Data Source=:memory:");

        public static async Task<UserManagementApiFactory> CreateAsync()
        {
            ConfigureEnvironment();
            var factory = new UserManagementApiFactory();
            await factory.connection.OpenAsync();
            await factory.InitializeDatabaseAsync();

            return factory;
        }

        public async Task<IReadOnlyList<string>> GetPasswordHashesAsync()
        {
            using var scope = Services.CreateScope();
            var dbContext = scope.ServiceProvider.GetRequiredService<OpsSphereDbContext>();

            return await dbContext.Users
                .AsNoTracking()
                .Select(user => user.PasswordHash)
                .ToArrayAsync();
        }

        public async Task AssertAuditAsync(string action, Guid entityId)
        {
            using var scope = Services.CreateScope();
            var dbContext = scope.ServiceProvider.GetRequiredService<OpsSphereDbContext>();

            var audit = await dbContext.AuditLogs
                .AsNoTracking()
                .Where(log => log.Action == action && log.EntityType == "User" && log.EntityId == entityId)
                .OrderByDescending(log => log.CreatedAt)
                .FirstOrDefaultAsync();

            Assert.NotNull(audit);
            Assert.Equal(SeedIds.Users.Admin, audit.ActorUserId);
            Assert.False(string.IsNullOrWhiteSpace(audit.CorrelationId));
            Assert.DoesNotContain("password", audit.PreviousValue ?? string.Empty, StringComparison.OrdinalIgnoreCase);
            Assert.DoesNotContain("password", audit.NewValue ?? string.Empty, StringComparison.OrdinalIgnoreCase);
            Assert.DoesNotContain(JwtSigningKey, audit.PreviousValue ?? string.Empty, StringComparison.Ordinal);
            Assert.DoesNotContain(JwtSigningKey, audit.NewValue ?? string.Empty, StringComparison.Ordinal);
        }

        protected override void ConfigureWebHost(IWebHostBuilder builder)
        {
            builder.UseEnvironment("Testing");
            builder.ConfigureAppConfiguration((_, config) =>
            {
                config.AddInMemoryCollection(new Dictionary<string, string?>
                {
                    ["ConnectionStrings:DefaultConnection"] = "Server=(local);Database=OpsSphereUserManagementTests;Trusted_Connection=True;TrustServerCertificate=True;",
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
                "Server=(local);Database=OpsSphereUserManagementTests;Trusted_Connection=True;TrustServerCertificate=True;");
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
