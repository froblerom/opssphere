using Microsoft.AspNetCore.Identity;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using OpsSphere.Domain.Entities;
using OpsSphere.Infrastructure.Persistence;
using OpsSphere.Infrastructure.Persistence.SeedData;

namespace OpsSphere.IntegrationTests.Persistence;

public sealed class SeedDataTests
{
    private static readonly string[] RequiredPermissionCodes =
    [
        "users.view",
        "users.manage",
        "roles.view",
        "roles.manage",
        "permissions.view",
        "permissions.manage",
        "scopes.view",
        "scopes.manage",
        "organization.view",
        "organization.manage",
        "regions.manage",
        "countries.manage",
        "accounts.manage",
        "campaigns.manage",
        "assignments.manage",
        "customers.view",
        "customers.create",
        "customers.update",
        "customers.history.view",
        "tickets.view",
        "tickets.create",
        "tickets.update",
        "tickets.assign",
        "tickets.update_status",
        "tickets.update_priority",
        "tickets.comment",
        "tickets.escalate",
        "tickets.resolve",
        "tickets.close",
        "tickets.history.view",
        "tickets.reopen",
        "sla.view",
        "sla.policies.view",
        "sla.policies.manage",
        "sla.evaluate",
        "dashboard.view",
        "reports.view",
        "reports.export",
        "audit.view",
        "audit.admin_view",
        "audit.export"
    ];

    private static readonly IReadOnlyDictionary<string, string[]> ExpectedRolePermissions = new Dictionary<string, string[]>
    {
        ["Admin"] =
        [
            "users.view", "users.manage", "roles.view", "roles.manage", "permissions.view", "permissions.manage",
            "scopes.view", "scopes.manage", "organization.view", "organization.manage", "regions.manage",
            "countries.manage", "accounts.manage", "campaigns.manage", "assignments.manage", "customers.view",
            "customers.create", "customers.update", "customers.history.view", "tickets.view", "tickets.history.view",
            "tickets.reopen", "sla.view", "sla.policies.view", "sla.policies.manage", "sla.evaluate", "dashboard.view",
            "reports.view", "reports.export", "audit.view", "audit.admin_view", "audit.export"
        ],
        ["OperationsManager"] =
        [
            "organization.view", "customers.view", "customers.history.view", "tickets.view", "tickets.history.view",
            "tickets.comment", "sla.view", "sla.policies.view", "dashboard.view", "reports.view", "reports.export",
            "audit.view", "audit.export"
        ],
        ["Supervisor"] =
        [
            "organization.view", "customers.view", "customers.create", "customers.update", "customers.history.view",
            "tickets.view", "tickets.create", "tickets.update", "tickets.assign", "tickets.update_status",
            "tickets.update_priority", "tickets.comment", "tickets.escalate", "tickets.resolve", "tickets.close",
            "tickets.history.view", "tickets.reopen", "sla.view", "sla.policies.view", "dashboard.view",
            "reports.view", "reports.export", "audit.view"
        ],
        ["Agent"] =
        [
            "organization.view", "customers.view", "customers.create", "customers.update", "customers.history.view",
            "tickets.view", "tickets.create", "tickets.update", "tickets.update_status", "tickets.update_priority",
            "tickets.comment", "tickets.escalate", "tickets.resolve", "tickets.close", "tickets.history.view",
            "sla.view", "dashboard.view"
        ],
        ["Viewer"] =
        [
            "organization.view", "customers.view", "customers.history.view", "tickets.view", "tickets.history.view",
            "sla.view", "dashboard.view", "reports.view", "reports.export", "audit.view"
        ]
    };

    [Fact]
    public async Task SeedData_WhenApplied_ShouldCreateRequiredRoles()
    {
        await using var database = await SeedDatabase.CreateAsync();

        var roleNames = await database.Context.Roles.Select(role => role.Name).ToListAsync();

        Assert.Contains("Admin", roleNames);
        Assert.Contains("OperationsManager", roleNames);
        Assert.Contains("Supervisor", roleNames);
        Assert.Contains("Agent", roleNames);
        Assert.Contains("Viewer", roleNames);
        Assert.All(await database.Context.Roles.ToListAsync(), role =>
        {
            Assert.True(role.IsSystemRole);
            Assert.True(role.IsActive);
        });
    }

    [Fact]
    public async Task SeedData_WhenApplied_ShouldCreateRequiredPermissions()
    {
        await using var database = await SeedDatabase.CreateAsync();

        var permissionCodes = await database.Context.Permissions
            .Select(permission => permission.Code)
            .ToListAsync();

        foreach (var expectedPermissionCode in RequiredPermissionCodes)
        {
            Assert.Contains(expectedPermissionCode, permissionCodes);
            Assert.Equal(expectedPermissionCode, expectedPermissionCode.ToLowerInvariant());
            Assert.Contains(".", expectedPermissionCode);
        }
    }

    [Fact]
    public async Task SeedData_WhenApplied_ShouldCreateRolePermissionMappings()
    {
        await using var database = await SeedDatabase.CreateAsync();

        var mappings = await database.Context.RolePermissions
            .Include(rolePermission => rolePermission.Role)
            .Include(rolePermission => rolePermission.Permission)
            .Select(rolePermission => new
            {
                RoleName = rolePermission.Role.Name,
                PermissionCode = rolePermission.Permission.Code
            })
            .ToListAsync();

        foreach (var (roleName, permissionCodes) in ExpectedRolePermissions)
        {
            foreach (var permissionCode in permissionCodes)
            {
                Assert.Contains(mappings, mapping =>
                    mapping.RoleName == roleName && mapping.PermissionCode == permissionCode);
            }
        }

        Assert.DoesNotContain(mappings, mapping =>
            mapping.RoleName == "Admin" && mapping.PermissionCode == "tickets.create");
        Assert.DoesNotContain(mappings, mapping =>
            mapping.RoleName == "Viewer" && mapping.PermissionCode is "tickets.create" or "tickets.update" or "tickets.close");
        Assert.Contains(mappings, mapping =>
            mapping.RoleName == "Supervisor" && mapping.PermissionCode == "tickets.assign");
        Assert.Contains(mappings, mapping =>
            mapping.RoleName == "Agent" && mapping.PermissionCode == "tickets.create");

        var adminTicketOperatorPermissions = new[]
        {
            "tickets.create", "tickets.update", "tickets.assign", "tickets.update_status", "tickets.update_priority",
            "tickets.comment", "tickets.escalate", "tickets.resolve", "tickets.close"
        };
        Assert.All(adminTicketOperatorPermissions, permissionCode =>
            Assert.DoesNotContain(mappings, mapping => mapping.RoleName == "Admin" && mapping.PermissionCode == permissionCode));

        var viewerWritePermissions = new[]
        {
            "users.manage", "roles.manage", "permissions.manage", "scopes.manage", "organization.manage",
            "regions.manage", "countries.manage", "accounts.manage", "campaigns.manage", "assignments.manage",
            "customers.create", "customers.update", "tickets.create", "tickets.update", "tickets.assign",
            "tickets.update_status", "tickets.update_priority", "tickets.comment", "tickets.escalate",
            "tickets.resolve", "tickets.close", "tickets.reopen", "sla.policies.manage", "sla.evaluate"
        };
        Assert.All(viewerWritePermissions, permissionCode =>
            Assert.DoesNotContain(mappings, mapping => mapping.RoleName == "Viewer" && mapping.PermissionCode == permissionCode));

        var operationsManagerDeniedPermissions = new[]
        {
            "users.manage", "roles.manage", "permissions.manage", "scopes.manage", "organization.manage",
            "tickets.create", "tickets.update", "tickets.assign", "tickets.update_status", "tickets.update_priority",
            "tickets.escalate", "tickets.resolve", "tickets.close", "tickets.reopen", "sla.policies.manage", "sla.evaluate"
        };
        Assert.All(operationsManagerDeniedPermissions, permissionCode =>
            Assert.DoesNotContain(mappings, mapping => mapping.RoleName == "OperationsManager" && mapping.PermissionCode == permissionCode));
    }

    [Fact]
    public async Task SeedData_WhenApplied_ShouldCreateFictionalUsersWithPasswordHashes()
    {
        await using var database = await SeedDatabase.CreateAsync();
        var passwordHasher = new PasswordHasher<User>();

        var users = await database.Context.Users.ToListAsync();

        foreach (var expectedUser in OpsSphereSeedData.Users)
        {
            var user = Assert.Single(users, user => user.Email == expectedUser.Email);
            Assert.True(user.IsActive);
            Assert.False(string.IsNullOrWhiteSpace(user.PasswordHash));
            Assert.NotEqual(OpsSphereSeedData.LocalDemoPassword, user.PasswordHash);

            var verificationResult = passwordHasher.VerifyHashedPassword(
                user,
                user.PasswordHash,
                OpsSphereSeedData.LocalDemoPassword);

            Assert.True(verificationResult is PasswordVerificationResult.Success or PasswordVerificationResult.SuccessRehashNeeded);
        }
    }

    [Fact]
    public async Task SeedData_WhenApplied_ShouldCreateOrganizationHierarchy()
    {
        await using var database = await SeedDatabase.CreateAsync();

        Assert.Equal(["LATAM", "NA"], await database.Context.Regions.OrderBy(region => region.Code).Select(region => region.Code).ToListAsync());
        Assert.Equal(["CO", "CR", "MX", "US"], await database.Context.Countries.OrderBy(country => country.Code).Select(country => country.Code).ToListAsync());
        Assert.Equal(["AEROLINK", "NOVABANK", "SHOPORA", "STREAMLY"], await database.Context.Accounts.OrderBy(account => account.Code).Select(account => account.Code).ToListAsync());
        Assert.Equal(
            ["AEROLINK-TRAVEL", "NOVABANK-CC", "NOVABANK-FRAUD", "SHOPORA-ACCESS", "STREAMLY-CREATOR"],
            await database.Context.Campaigns.OrderBy(campaign => campaign.Code).Select(campaign => campaign.Code).ToListAsync());

        var novaBank = await database.Context.Accounts.SingleAsync(account => account.Code == "NOVABANK");
        var mexico = await database.Context.Countries.SingleAsync(country => country.Code == "MX");
        Assert.Equal(mexico.Id, novaBank.CountryId);

        var novaBankCreditCard = await database.Context.Campaigns.SingleAsync(campaign => campaign.Code == "NOVABANK-CC");
        Assert.Equal(novaBank.Id, novaBankCreditCard.AccountId);
        Assert.Equal(mexico.Id, novaBankCreditCard.CountryId);
    }

    [Fact]
    public async Task SeedData_WhenApplied_ShouldCreateInitialUserScopes()
    {
        await using var database = await SeedDatabase.CreateAsync();

        Assert.False(await database.Context.UserScopes
            .AnyAsync(scope => scope.User.Email == "admin@opssphere.local"));

        await AssertScopeAsync(database.Context, "manager.latam@opssphere.local", "Region", "LATAM");
        await AssertScopeAsync(database.Context, "supervisor.novabank@opssphere.local", "Account", "NOVABANK");
        await AssertScopeAsync(database.Context, "agent.novabank@opssphere.local", "Campaign", "NOVABANK-CC");
        await AssertScopeAsync(database.Context, "viewer.latam@opssphere.local", "Region", "LATAM");
    }

    [Fact]
    public async Task SeedData_WhenAppliedTwice_ShouldNotCreateDuplicates()
    {
        await using var database = await SeedDatabase.CreateAsync();
        var seeder = new OpsSphereDataSeeder(database.Context, new PasswordHasher<User>());

        await seeder.SeedAsync();

        Assert.Equal(5, await database.Context.Roles.CountAsync());
        Assert.Equal(RequiredPermissionCodes.Length, await database.Context.Permissions.CountAsync());
        Assert.Equal(5, await database.Context.Users.CountAsync());
        Assert.Equal(2, await database.Context.Regions.CountAsync());
        Assert.Equal(4, await database.Context.Countries.CountAsync());
        Assert.Equal(4, await database.Context.Accounts.CountAsync());
        Assert.Equal(5, await database.Context.Campaigns.CountAsync());
        Assert.Equal(4, await database.Context.UserScopes.CountAsync());

        var expectedRolePermissionCount = ExpectedRolePermissions.Sum(mapping => mapping.Value.Length);
        Assert.Equal(expectedRolePermissionCount, await database.Context.RolePermissions.CountAsync());
        Assert.Equal(5, await database.Context.UserRoles.CountAsync());
    }

    [Fact]
    public async Task SeedData_ShouldNotStorePlainTextPasswords()
    {
        await using var database = await SeedDatabase.CreateAsync();

        var passwordHashes = await database.Context.Users.Select(user => user.PasswordHash).ToListAsync();

        Assert.All(passwordHashes, hash =>
        {
            Assert.NotEqual(OpsSphereSeedData.LocalDemoPassword, hash);
            Assert.DoesNotContain("OpsSphere123!", hash, StringComparison.Ordinal);
        });
    }

    [Fact]
    public async Task SeedData_ShouldNotContainObviousRealOrProductionSecrets()
    {
        await using var database = await SeedDatabase.CreateAsync();

        var seededValues = new List<string>();
        seededValues.AddRange(await database.Context.Users.Select(user => user.Email).ToListAsync());
        seededValues.AddRange(await database.Context.Users.Select(user => user.DisplayName).ToListAsync());
        seededValues.AddRange(await database.Context.Regions.Select(region => region.Name).ToListAsync());
        seededValues.AddRange(await database.Context.Countries.Select(country => country.Name).ToListAsync());
        seededValues.AddRange(await database.Context.Accounts.Select(account => account.Name).ToListAsync());
        seededValues.AddRange(await database.Context.Accounts.Select(account => account.Description ?? string.Empty).ToListAsync());
        seededValues.AddRange(await database.Context.Campaigns.Select(campaign => campaign.Name).ToListAsync());
        seededValues.AddRange(await database.Context.Campaigns.Select(campaign => campaign.Description ?? string.Empty).ToListAsync());

        var forbiddenMarkers = new[] { "production", "prod-", "secret", "token", "api key", "password hash" };

        foreach (var value in seededValues.Where(value => !string.IsNullOrWhiteSpace(value)))
        {
            foreach (var marker in forbiddenMarkers)
            {
                Assert.DoesNotContain(marker, value, StringComparison.OrdinalIgnoreCase);
            }
        }
    }

    private static async Task AssertScopeAsync(OpsSphereDbContext context, string email, string scopeType, string scopeCode)
    {
        var scope = await context.UserScopes
            .Include(userScope => userScope.User)
            .Include(userScope => userScope.Region)
            .Include(userScope => userScope.Account)
            .Include(userScope => userScope.Campaign)
            .SingleAsync(userScope => userScope.User.Email == email && userScope.ScopeType == scopeType);

        Assert.True(scope.IsActive);

        var actualCode = scopeType switch
        {
            "Region" => scope.Region?.Code,
            "Account" => scope.Account?.Code,
            "Campaign" => scope.Campaign?.Code,
            _ => null
        };

        Assert.Equal(scopeCode, actualCode);
    }

    private sealed class SeedDatabase : IAsyncDisposable
    {
        private SeedDatabase(SqliteConnection connection, OpsSphereDbContext context)
        {
            Connection = connection;
            Context = context;
        }

        private SqliteConnection Connection { get; }

        public OpsSphereDbContext Context { get; }

        public static async Task<SeedDatabase> CreateAsync()
        {
            var connection = new SqliteConnection("Data Source=:memory:");
            await connection.OpenAsync();

            var options = new DbContextOptionsBuilder<OpsSphereDbContext>()
                .UseSqlite(connection)
                .Options;

            var context = new OpsSphereDbContext(options);
            await context.Database.EnsureCreatedAsync();

            var seeder = new OpsSphereDataSeeder(context, new PasswordHasher<User>());
            await seeder.SeedAsync();

            return new SeedDatabase(connection, context);
        }

        public async ValueTask DisposeAsync()
        {
            await Context.DisposeAsync();
            await Connection.DisposeAsync();
        }
    }
}
