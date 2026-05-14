using Microsoft.EntityFrameworkCore;
using OpsSphere.Application.Common.Interfaces;
using OpsSphere.Application.Features.Auth;
using OpsSphere.Infrastructure.Persistence;

namespace OpsSphere.Infrastructure.Identity;

internal sealed class AuthUserReader : IAuthUserReader
{
    private readonly OpsSphereDbContext dbContext;

    public AuthUserReader(OpsSphereDbContext dbContext)
    {
        this.dbContext = dbContext;
    }

    public async Task<AuthUserCredentialUser?> FindCredentialUserByEmailAsync(string email, CancellationToken cancellationToken)
    {
        var user = await dbContext.Users
            .AsNoTracking()
            .Include(u => u.UserRoles)
                .ThenInclude(ur => ur.Role)
            .FirstOrDefaultAsync(u => u.Email == email, cancellationToken);

        if (user is null)
        {
            return null;
        }

        var roles = user.UserRoles
            .Where(userRole => userRole.Role.IsActive)
            .Select(userRole => userRole.Role.Name)
            .Distinct(StringComparer.Ordinal)
            .Order(StringComparer.Ordinal)
            .ToArray();

        return new AuthUserCredentialUser(user.Id, user.Email, user.PasswordHash, user.DisplayName, user.IsActive, roles);
    }

    public async Task<AuthUserProfile?> GetUserProfileAsync(Guid userId, CancellationToken cancellationToken)
    {
        var user = await dbContext.Users
            .AsNoTracking()
            .Include(u => u.UserRoles)
                .ThenInclude(ur => ur.Role)
            .Include(u => u.UserScopes.Where(us => us.IsActive))
                .ThenInclude(us => us.Region)
            .Include(u => u.UserScopes.Where(us => us.IsActive))
                .ThenInclude(us => us.Country)
            .Include(u => u.UserScopes.Where(us => us.IsActive))
                .ThenInclude(us => us.Account)
            .Include(u => u.UserScopes.Where(us => us.IsActive))
                .ThenInclude(us => us.Campaign)
            .FirstOrDefaultAsync(u => u.Id == userId, cancellationToken);

        if (user is null)
        {
            return null;
        }

        var activeRoleIds = user.UserRoles
            .Where(userRole => userRole.Role.IsActive)
            .Select(userRole => userRole.RoleId)
            .Distinct()
            .ToArray();

        var roles = user.UserRoles
            .Where(userRole => userRole.Role.IsActive)
            .Select(userRole => userRole.Role.Name)
            .Distinct(StringComparer.Ordinal)
            .Order(StringComparer.Ordinal)
            .ToArray();

        var permissions = await dbContext.RolePermissions
            .AsNoTracking()
            .Where(rolePermission => activeRoleIds.Contains(rolePermission.RoleId) && rolePermission.Permission.IsActive)
            .Select(rolePermission => rolePermission.Permission.Code)
            .Distinct()
            .OrderBy(code => code)
            .ToArrayAsync(cancellationToken);

        var scopes = user.UserScopes
            .Where(userScope => userScope.IsActive)
            .Select(userScope => new UserScopeDto(
                userScope.ScopeType,
                userScope.RegionId,
                userScope.Region?.Code,
                userScope.CountryId,
                userScope.Country?.Code,
                userScope.AccountId,
                userScope.Account?.Code,
                userScope.CampaignId,
                userScope.Campaign?.Code))
            .OrderBy(scope => scope.ScopeType, StringComparer.Ordinal)
            .ThenBy(scope => scope.RegionCode, StringComparer.Ordinal)
            .ThenBy(scope => scope.CountryCode, StringComparer.Ordinal)
            .ThenBy(scope => scope.AccountCode, StringComparer.Ordinal)
            .ThenBy(scope => scope.CampaignCode, StringComparer.Ordinal)
            .ToArray();

        return new AuthUserProfile(user.Id, user.Email, user.DisplayName, user.IsActive, roles, permissions, scopes);
    }

    public async Task UpdateLastLoginAtAsync(Guid userId, DateTime lastLoginAt, CancellationToken cancellationToken)
    {
        var user = await dbContext.Users.FirstOrDefaultAsync(u => u.Id == userId, cancellationToken);
        if (user is not null)
        {
            user.LastLoginAt = lastLoginAt;
        }
    }
}
