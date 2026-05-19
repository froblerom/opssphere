using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Reflection;
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

namespace OpsSphere.IntegrationTests.TestInfrastructure;

public sealed class OpsSphereSqliteFactory : WebApplicationFactory<Program>, IAsyncDisposable
{
    public const string JwtSigningKey = "integration-testing-only-fictional-jwt-signing-key";

    private static readonly JsonSerializerOptions JsonOptions = new(JsonSerializerDefaults.Web);

    private readonly SqliteConnection connection = new("Data Source=:memory:");

    public static async Task<OpsSphereSqliteFactory> CreateAsync()
    {
        ConfigureEnvironment();
        var factory = new OpsSphereSqliteFactory();
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
                ["ConnectionStrings:DefaultConnection"] = "Server=(local);Database=OpsSphereSqliteTests;Trusted_Connection=True;TrustServerCertificate=True;",
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
            services.AddControllers()
                .AddApplicationPart(Assembly.GetExecutingAssembly());
        });
    }

    private static void ConfigureEnvironment()
    {
        Environment.SetEnvironmentVariable("ConnectionStrings__DefaultConnection", "Server=(local);Database=OpsSphereSqliteTests;Trusted_Connection=True;TrustServerCertificate=True;");
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

    public async Task<HttpClient> CreateAuthenticatedClientAsync(string email)
    {
        var client = CreateClient();
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

    public static async Task<T> ReadDataAsync<T>(HttpResponseMessage response)
    {
        var envelope = await ReadResponseAsync<ApiResponse<T>>(response);
        return envelope.Data;
    }

    public static async Task<ApiErrorResponse> ReadErrorAsync(HttpResponseMessage response) =>
        await ReadResponseAsync<ApiErrorResponse>(response);

    public static async Task<T> ReadResponseAsync<T>(HttpResponseMessage response)
    {
        var json = await response.Content.ReadAsStringAsync();
        var value = JsonSerializer.Deserialize<T>(json, JsonOptions);
        return value ?? throw new InvalidOperationException($"Could not deserialize {typeof(T).Name} from {(int)response.StatusCode}: {json}");
    }

    public async Task AssertAuditAsync(string action, Guid entityId)
    {
        using var scope = Services.CreateScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<OpsSphereDbContext>();
        var audit = await dbContext.AuditLogs
            .AsNoTracking()
            .Where(log => log.Action == action && log.EntityId == entityId)
            .OrderByDescending(log => log.CreatedAt)
            .FirstOrDefaultAsync();
        Assert.NotNull(audit);
        Assert.False(string.IsNullOrWhiteSpace(audit.CorrelationId));
    }

    public async Task AssertAuditDoesNotContainPiiAsync(Guid entityId)
    {
        using var scope = Services.CreateScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<OpsSphereDbContext>();
        var auditLogs = await dbContext.AuditLogs
            .AsNoTracking()
            .Where(log => log.EntityId == entityId)
            .ToListAsync();
        foreach (var log in auditLogs)
        {
            Assert.DoesNotContain("@", log.NewValue ?? string.Empty, StringComparison.Ordinal);
            Assert.DoesNotContain("phone", log.NewValue ?? string.Empty, StringComparison.OrdinalIgnoreCase);
            Assert.DoesNotContain("@", log.PreviousValue ?? string.Empty, StringComparison.Ordinal);
            Assert.DoesNotContain("phone", log.PreviousValue ?? string.Empty, StringComparison.OrdinalIgnoreCase);
        }
    }

    public async Task<Guid> AddInactiveRegionAsync(string code, string name)
    {
        using var scope = Services.CreateScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<OpsSphereDbContext>();
        var region = new Region { Id = Guid.NewGuid(), Code = code.ToUpperInvariant(), Name = name, IsActive = false, CreatedAt = DateTime.UtcNow };
        dbContext.Regions.Add(region);
        await dbContext.SaveChangesAsync();
        return region.Id;
    }

    public async Task<Guid> AddInactiveCountryAsync(Guid regionId, string code, string name)
    {
        using var scope = Services.CreateScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<OpsSphereDbContext>();
        var country = new Country { Id = Guid.NewGuid(), RegionId = regionId, Code = code.ToUpperInvariant(), Name = name, IsActive = false, CreatedAt = DateTime.UtcNow };
        dbContext.Countries.Add(country);
        await dbContext.SaveChangesAsync();
        return country.Id;
    }

    public async Task<Guid> AddInactiveAccountAsync(Guid countryId, string code, string name)
    {
        using var scope = Services.CreateScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<OpsSphereDbContext>();
        var account = new Account { Id = Guid.NewGuid(), CountryId = countryId, Code = code.ToUpperInvariant(), Name = name, IsActive = false, CreatedAt = DateTime.UtcNow };
        dbContext.Accounts.Add(account);
        await dbContext.SaveChangesAsync();
        return account.Id;
    }

    public async Task<Guid> AddInactiveAccountAsync(string code, string name)
    {
        return await AddInactiveAccountAsync(SeedIds.Countries.Mexico, code, name);
    }

    public async Task<Guid> GetUserIdAsync(string email)
    {
        using var scope = Services.CreateScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<OpsSphereDbContext>();
        var user = await dbContext.Users.AsNoTracking().SingleAsync(u => u.Email == email);
        return user.Id;
    }

    public async Task DeactivateUserAsync(Guid userId)
    {
        using var scope = Services.CreateScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<OpsSphereDbContext>();
        var user = await dbContext.Users.SingleAsync(u => u.Id == userId);
        user.IsActive = false;
        user.DeactivatedAt = DateTime.UtcNow;
        await dbContext.SaveChangesAsync();
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

    public async Task<IReadOnlyList<string>> GetPasswordHashesAsync()
    {
        using var scope = Services.CreateScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<OpsSphereDbContext>();
        return await dbContext.Users.AsNoTracking().Select(u => u.PasswordHash).ToListAsync();
    }

    public sealed record ApiResponse<T>(T Data);
    public sealed record ApiErrorResponse(ApiError Error);
    public sealed record ApiError(string Code, string Message, IReadOnlyList<ApiErrorDetail>? Details, string? CorrelationId);
    public sealed record ApiErrorDetail(string? Field, string Message);
    public sealed record LoginData(string AccessToken);
}
