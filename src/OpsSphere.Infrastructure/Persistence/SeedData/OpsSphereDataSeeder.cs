using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using OpsSphere.Domain.Entities;

namespace OpsSphere.Infrastructure.Persistence.SeedData;

public sealed class OpsSphereDataSeeder
{
    private readonly OpsSphereDbContext dbContext;
    private readonly IPasswordHasher<User> passwordHasher;

    public OpsSphereDataSeeder(OpsSphereDbContext dbContext, IPasswordHasher<User> passwordHasher)
    {
        this.dbContext = dbContext;
        this.passwordHasher = passwordHasher;
    }

    public async Task SeedAsync(CancellationToken cancellationToken = default)
    {
        await SeedRolesAsync(cancellationToken);
        await SeedPermissionsAsync(cancellationToken);
        await SeedRolePermissionsAsync(cancellationToken);
        await SeedOrganizationAsync(cancellationToken);
        await SeedCustomersAsync(cancellationToken);
        await SeedUsersAsync(cancellationToken);
        await SeedUserRolesAsync(cancellationToken);
        await SeedUserScopesAsync(cancellationToken);
    }

    private async Task SeedRolesAsync(CancellationToken cancellationToken)
    {
        var existingRoles = await LoadRolesByNameAsync(cancellationToken);

        foreach (var roleSeed in OpsSphereSeedData.Roles)
        {
            if (!existingRoles.TryGetValue(roleSeed.Name, out var role))
            {
                dbContext.Roles.Add(new Role
                {
                    Id = roleSeed.Id,
                    Name = roleSeed.Name,
                    Description = roleSeed.Description,
                    IsSystemRole = true,
                    IsActive = true
                });
                continue;
            }

            role.Description = roleSeed.Description;
            role.IsSystemRole = true;
            role.IsActive = true;
        }

        await dbContext.SaveChangesAsync(cancellationToken);
    }

    private async Task SeedPermissionsAsync(CancellationToken cancellationToken)
    {
        var existingPermissions = await LoadPermissionsByCodeAsync(cancellationToken);

        foreach (var permissionSeed in OpsSphereSeedData.Permissions)
        {
            if (!existingPermissions.TryGetValue(permissionSeed.Code, out var permission))
            {
                dbContext.Permissions.Add(new Permission
                {
                    Id = permissionSeed.Id,
                    Code = permissionSeed.Code,
                    Name = permissionSeed.Name,
                    Description = permissionSeed.Description,
                    IsActive = true
                });
                continue;
            }

            permission.Name = permissionSeed.Name;
            permission.Description = permissionSeed.Description;
            permission.IsActive = true;
        }

        await dbContext.SaveChangesAsync(cancellationToken);
    }

    private async Task SeedRolePermissionsAsync(CancellationToken cancellationToken)
    {
        var roles = await LoadRolesByNameAsync(cancellationToken);
        var permissions = await LoadPermissionsByCodeAsync(cancellationToken);
        var existingMappings = await dbContext.RolePermissions
            .Select(rp => new { rp.RoleId, rp.PermissionId })
            .ToListAsync(cancellationToken);
        var mappingKeys = existingMappings
            .Select(rp => (rp.RoleId, rp.PermissionId))
            .ToHashSet();

        foreach (var (roleName, permissionCodes) in OpsSphereSeedData.RolePermissions)
        {
            var role = roles[roleName];

            foreach (var permissionCode in permissionCodes)
            {
                var permission = permissions[permissionCode];
                var key = (role.Id, permission.Id);

                if (mappingKeys.Contains(key))
                {
                    continue;
                }

                dbContext.RolePermissions.Add(new RolePermission
                {
                    RoleId = role.Id,
                    PermissionId = permission.Id
                });
                mappingKeys.Add(key);
            }
        }

        await dbContext.SaveChangesAsync(cancellationToken);
    }

    private async Task SeedOrganizationAsync(CancellationToken cancellationToken)
    {
        await SeedRegionsAsync(cancellationToken);
        await SeedCountriesAsync(cancellationToken);
        await SeedAccountsAsync(cancellationToken);
        await SeedCampaignsAsync(cancellationToken);
    }

    private async Task SeedRegionsAsync(CancellationToken cancellationToken)
    {
        var regions = await LoadRegionsByCodeAsync(cancellationToken);

        foreach (var regionSeed in OpsSphereSeedData.Regions)
        {
            if (!regions.TryGetValue(regionSeed.Code, out var region))
            {
                dbContext.Regions.Add(new Region
                {
                    Id = regionSeed.Id,
                    Code = regionSeed.Code,
                    Name = regionSeed.Name,
                    IsActive = true
                });
                continue;
            }

            region.Name = regionSeed.Name;
            region.IsActive = true;
        }

        await dbContext.SaveChangesAsync(cancellationToken);
    }

    private async Task SeedCountriesAsync(CancellationToken cancellationToken)
    {
        var regions = await LoadRegionsByCodeAsync(cancellationToken);
        var countries = await LoadCountriesByCodeAsync(cancellationToken);

        foreach (var countrySeed in OpsSphereSeedData.Countries)
        {
            var regionId = regions[countrySeed.RegionCode].Id;

            if (!countries.TryGetValue(countrySeed.Code, out var country))
            {
                dbContext.Countries.Add(new Country
                {
                    Id = countrySeed.Id,
                    RegionId = regionId,
                    Code = countrySeed.Code,
                    Name = countrySeed.Name,
                    IsActive = true
                });
                continue;
            }

            country.RegionId = regionId;
            country.Name = countrySeed.Name;
            country.IsActive = true;
        }

        await dbContext.SaveChangesAsync(cancellationToken);
    }

    private async Task SeedAccountsAsync(CancellationToken cancellationToken)
    {
        var countries = await LoadCountriesByCodeAsync(cancellationToken);
        var accounts = await LoadAccountsByCodeAsync(cancellationToken);

        foreach (var accountSeed in OpsSphereSeedData.Accounts)
        {
            var countryId = countries[accountSeed.CountryCode].Id;

            if (!accounts.TryGetValue(accountSeed.Code, out var account))
            {
                dbContext.Accounts.Add(new Account
                {
                    Id = accountSeed.Id,
                    CountryId = countryId,
                    Code = accountSeed.Code,
                    Name = accountSeed.Name,
                    Description = accountSeed.Description,
                    IsActive = true
                });
                continue;
            }

            account.CountryId = countryId;
            account.Name = accountSeed.Name;
            account.Description = accountSeed.Description;
            account.IsActive = true;
        }

        await dbContext.SaveChangesAsync(cancellationToken);
    }

    private async Task SeedCampaignsAsync(CancellationToken cancellationToken)
    {
        var countries = await LoadCountriesByCodeAsync(cancellationToken);
        var accounts = await LoadAccountsByCodeAsync(cancellationToken);
        var campaigns = await LoadCampaignsByCodeAsync(cancellationToken);

        foreach (var campaignSeed in OpsSphereSeedData.Campaigns)
        {
            var accountId = accounts[campaignSeed.AccountCode].Id;
            var countryId = countries[campaignSeed.CountryCode].Id;

            if (!campaigns.TryGetValue(campaignSeed.Code, out var campaign))
            {
                dbContext.Campaigns.Add(new Campaign
                {
                    Id = campaignSeed.Id,
                    AccountId = accountId,
                    CountryId = countryId,
                    Code = campaignSeed.Code,
                    Name = campaignSeed.Name,
                    Description = campaignSeed.Description,
                    IsActive = true
                });
                continue;
            }

            campaign.AccountId = accountId;
            campaign.CountryId = countryId;
            campaign.Name = campaignSeed.Name;
            campaign.Description = campaignSeed.Description;
            campaign.IsActive = true;
        }

        await dbContext.SaveChangesAsync(cancellationToken);
    }

    private async Task SeedCustomersAsync(CancellationToken cancellationToken)
    {
        var accounts = await LoadAccountsByCodeAsync(cancellationToken);
        var existingCustomerIds = await dbContext.Customers
            .Select(c => c.Id)
            .ToHashSetAsync(cancellationToken);

        foreach (var customerSeed in OpsSphereSeedData.Customers)
        {
            if (existingCustomerIds.Contains(customerSeed.Id)) continue;

            var accountId = accounts[customerSeed.AccountCode].Id;
            dbContext.Customers.Add(new OpsSphere.Domain.Entities.Customer
            {
                Id = customerSeed.Id,
                AccountId = accountId,
                FirstName = customerSeed.FirstName,
                LastName = customerSeed.LastName,
                Email = customerSeed.Email,
                PhoneNumber = customerSeed.PhoneNumber,
                ExternalReference = customerSeed.ExternalReference,
                IsActive = true,
                IsDeleted = false,
                CreatedAt = DateTime.UtcNow
            });
        }

        await dbContext.SaveChangesAsync(cancellationToken);
    }

    private async Task SeedUsersAsync(CancellationToken cancellationToken)
    {
        var users = await LoadUsersByEmailAsync(cancellationToken);

        foreach (var userSeed in OpsSphereSeedData.Users)
        {
            if (!users.TryGetValue(userSeed.Email, out var user))
            {
                user = new User
                {
                    Id = userSeed.Id,
                    Email = userSeed.Email,
                    FirstName = userSeed.FirstName,
                    LastName = userSeed.LastName,
                    DisplayName = userSeed.DisplayName,
                    IsActive = true
                };
                user.PasswordHash = passwordHasher.HashPassword(user, OpsSphereSeedData.LocalDemoPassword);
                dbContext.Users.Add(user);
                continue;
            }

            user.FirstName = userSeed.FirstName;
            user.LastName = userSeed.LastName;
            user.DisplayName = userSeed.DisplayName;
            user.IsActive = true;
            user.DeactivatedAt = null;

            if (string.IsNullOrWhiteSpace(user.PasswordHash) ||
                string.Equals(user.PasswordHash, OpsSphereSeedData.LocalDemoPassword, StringComparison.Ordinal))
            {
                user.PasswordHash = passwordHasher.HashPassword(user, OpsSphereSeedData.LocalDemoPassword);
            }
        }

        await dbContext.SaveChangesAsync(cancellationToken);
    }

    private async Task SeedUserRolesAsync(CancellationToken cancellationToken)
    {
        var users = await LoadUsersByEmailAsync(cancellationToken);
        var roles = await LoadRolesByNameAsync(cancellationToken);
        var existingMappings = await dbContext.UserRoles
            .Select(ur => new { ur.UserId, ur.RoleId })
            .ToListAsync(cancellationToken);
        var mappingKeys = existingMappings
            .Select(ur => (ur.UserId, ur.RoleId))
            .ToHashSet();

        foreach (var userSeed in OpsSphereSeedData.Users)
        {
            var user = users[userSeed.Email];
            var role = roles[userSeed.RoleName];
            var key = (user.Id, role.Id);

            if (mappingKeys.Contains(key))
            {
                continue;
            }

            dbContext.UserRoles.Add(new UserRole
            {
                UserId = user.Id,
                RoleId = role.Id
            });
            mappingKeys.Add(key);
        }

        await dbContext.SaveChangesAsync(cancellationToken);
    }

    private async Task SeedUserScopesAsync(CancellationToken cancellationToken)
    {
        var users = await LoadUsersByEmailAsync(cancellationToken);
        var regions = await LoadRegionsByCodeAsync(cancellationToken);
        var countries = await LoadCountriesByCodeAsync(cancellationToken);
        var accounts = await LoadAccountsByCodeAsync(cancellationToken);
        var campaigns = await LoadCampaignsByCodeAsync(cancellationToken);

        foreach (var scopeSeed in OpsSphereSeedData.UserScopes)
        {
            var user = users[scopeSeed.UserEmail];
            var regionId = scopeSeed.RegionCode is null ? null : (Guid?)regions[scopeSeed.RegionCode].Id;
            var countryId = scopeSeed.CountryCode is null ? null : (Guid?)countries[scopeSeed.CountryCode].Id;
            var accountId = scopeSeed.AccountCode is null ? null : (Guid?)accounts[scopeSeed.AccountCode].Id;
            var campaignId = scopeSeed.CampaignCode is null ? null : (Guid?)campaigns[scopeSeed.CampaignCode].Id;

            var existingScope = await dbContext.UserScopes.FirstOrDefaultAsync(
                userScope =>
                    userScope.UserId == user.Id &&
                    userScope.ScopeType == scopeSeed.ScopeType &&
                    userScope.RegionId == regionId &&
                    userScope.CountryId == countryId &&
                    userScope.AccountId == accountId &&
                    userScope.CampaignId == campaignId,
                cancellationToken);

            if (existingScope is null)
            {
                dbContext.UserScopes.Add(new UserScope
                {
                    Id = scopeSeed.Id,
                    UserId = user.Id,
                    ScopeType = scopeSeed.ScopeType,
                    RegionId = regionId,
                    CountryId = countryId,
                    AccountId = accountId,
                    CampaignId = campaignId,
                    IsActive = true
                });
                continue;
            }

            existingScope.IsActive = true;
        }

        await dbContext.SaveChangesAsync(cancellationToken);
    }

    private async Task<Dictionary<string, Role>> LoadRolesByNameAsync(CancellationToken cancellationToken) =>
        ToCaseInsensitiveDictionary(await dbContext.Roles.ToListAsync(cancellationToken), role => role.Name);

    private async Task<Dictionary<string, Permission>> LoadPermissionsByCodeAsync(CancellationToken cancellationToken) =>
        ToCaseInsensitiveDictionary(await dbContext.Permissions.ToListAsync(cancellationToken), permission => permission.Code);

    private async Task<Dictionary<string, User>> LoadUsersByEmailAsync(CancellationToken cancellationToken) =>
        ToCaseInsensitiveDictionary(await dbContext.Users.ToListAsync(cancellationToken), user => user.Email);

    private async Task<Dictionary<string, Region>> LoadRegionsByCodeAsync(CancellationToken cancellationToken) =>
        ToCaseInsensitiveDictionary(await dbContext.Regions.ToListAsync(cancellationToken), region => region.Code);

    private async Task<Dictionary<string, Country>> LoadCountriesByCodeAsync(CancellationToken cancellationToken) =>
        ToCaseInsensitiveDictionary(await dbContext.Countries.ToListAsync(cancellationToken), country => country.Code);

    private async Task<Dictionary<string, Account>> LoadAccountsByCodeAsync(CancellationToken cancellationToken) =>
        ToCaseInsensitiveDictionary(await dbContext.Accounts.ToListAsync(cancellationToken), account => account.Code);

    private async Task<Dictionary<string, Campaign>> LoadCampaignsByCodeAsync(CancellationToken cancellationToken) =>
        ToCaseInsensitiveDictionary(await dbContext.Campaigns.ToListAsync(cancellationToken), campaign => campaign.Code);

    private static Dictionary<string, T> ToCaseInsensitiveDictionary<T>(IEnumerable<T> items, Func<T, string> keySelector) =>
        items.ToDictionary(keySelector, StringComparer.OrdinalIgnoreCase);
}
