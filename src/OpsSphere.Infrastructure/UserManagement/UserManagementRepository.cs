using Microsoft.EntityFrameworkCore;
using OpsSphere.Application.Common.Interfaces;
using OpsSphere.Application.Features.UserManagement;
using OpsSphere.Domain.Entities;
using OpsSphere.Infrastructure.Persistence;

namespace OpsSphere.Infrastructure.UserManagement;

internal sealed class UserManagementRepository : IUserManagementRepository
{
    private readonly OpsSphereDbContext dbContext;

    public UserManagementRepository(OpsSphereDbContext dbContext)
    {
        this.dbContext = dbContext;
    }

    public async Task<IReadOnlyList<UserSummaryDto>> GetUsersAsync(CancellationToken cancellationToken)
    {
        var users = await dbContext.Users
            .AsNoTracking()
            .Include(u => u.UserRoles)
                .ThenInclude(ur => ur.Role)
            .OrderBy(u => u.Email)
            .ToListAsync(cancellationToken);

        return users.Select(u => new UserSummaryDto(
            u.Id,
            u.Email,
            u.FirstName,
            u.LastName,
            u.DisplayName,
            u.IsActive,
            u.CreatedAt,
            u.UpdatedAt,
            u.LastLoginAt,
            MapRoles(u.UserRoles))).ToArray();
    }

    public async Task<UserDetailDto?> GetUserByIdAsync(Guid userId, CancellationToken cancellationToken)
    {
        var user = await dbContext.Users
            .AsNoTracking()
            .Include(u => u.UserRoles)
                .ThenInclude(ur => ur.Role)
            .FirstOrDefaultAsync(u => u.Id == userId, cancellationToken);

        return user is null
            ? null
            : new UserDetailDto(
                user.Id,
                user.Email,
                user.FirstName,
                user.LastName,
                user.DisplayName,
                user.IsActive,
                user.CreatedAt,
                user.UpdatedAt,
                user.LastLoginAt,
                user.DeactivatedAt,
                MapRoles(user.UserRoles));
    }

    public Task<bool> EmailExistsAsync(string email, Guid? excludingUserId, CancellationToken cancellationToken) =>
        dbContext.Users.AnyAsync(
            u => u.Email == email && (!excludingUserId.HasValue || u.Id != excludingUserId.Value),
            cancellationToken);

    public Task AddUserAsync(UserCreateModel user, CancellationToken cancellationToken)
    {
        dbContext.Users.Add(new User
        {
            Id = user.Id,
            Email = user.Email,
            PasswordHash = user.PasswordHash,
            FirstName = user.FirstName,
            LastName = user.LastName,
            DisplayName = user.DisplayName,
            IsActive = user.IsActive,
            CreatedAt = user.CreatedAt
        });

        return Task.CompletedTask;
    }

    public async Task<UserUpdateSnapshot?> GetUserUpdateSnapshotAsync(Guid userId, CancellationToken cancellationToken) =>
        await dbContext.Users
            .AsNoTracking()
            .Where(u => u.Id == userId)
            .Select(u => new UserUpdateSnapshot(u.Id, u.Email, u.FirstName, u.LastName, u.DisplayName))
            .FirstOrDefaultAsync(cancellationToken);

    public async Task UpdateUserAsync(Guid userId, string email, string firstName, string lastName, string displayName, CancellationToken cancellationToken)
    {
        var user = await dbContext.Users.FirstAsync(u => u.Id == userId, cancellationToken);
        user.Email = email;
        user.FirstName = firstName;
        user.LastName = lastName;
        user.DisplayName = displayName;
    }

    public async Task<UserStatusSnapshot?> GetUserStatusSnapshotAsync(Guid userId, CancellationToken cancellationToken) =>
        await dbContext.Users
            .AsNoTracking()
            .Where(u => u.Id == userId)
            .Select(u => new UserStatusSnapshot(u.Id, u.IsActive))
            .FirstOrDefaultAsync(cancellationToken);

    public async Task DeactivateUserAsync(Guid userId, DateTime deactivatedAt, CancellationToken cancellationToken)
    {
        var user = await dbContext.Users.FirstAsync(u => u.Id == userId, cancellationToken);
        user.IsActive = false;
        user.DeactivatedAt = deactivatedAt;
    }

    public async Task<IReadOnlyList<RoleDto>> GetRolesAsync(CancellationToken cancellationToken) =>
        await dbContext.Roles
            .AsNoTracking()
            .OrderBy(r => r.Name)
            .Select(r => new RoleDto(r.Id, r.Name, r.Description, r.IsSystemRole, r.IsActive))
            .ToArrayAsync(cancellationToken);

    public async Task<IReadOnlyList<PermissionDto>> GetPermissionsAsync(CancellationToken cancellationToken) =>
        await dbContext.Permissions
            .AsNoTracking()
            .OrderBy(p => p.Code)
            .Select(p => new PermissionDto(p.Id, p.Code, p.Name, p.Description, p.IsActive))
            .ToArrayAsync(cancellationToken);

    public async Task<IReadOnlyList<Guid>> GetMissingActiveRoleIdsAsync(IReadOnlyCollection<Guid> roleIds, CancellationToken cancellationToken)
    {
        var existingRoleIds = await dbContext.Roles
            .AsNoTracking()
            .Where(r => roleIds.Contains(r.Id) && r.IsActive)
            .Select(r => r.Id)
            .ToArrayAsync(cancellationToken);

        return roleIds.Except(existingRoleIds).ToArray();
    }

    public async Task<IReadOnlyList<RoleAssignmentChange>> ReplaceUserRolesAsync(
        Guid userId,
        IReadOnlyCollection<Guid> roleIds,
        Guid? actorUserId,
        CancellationToken cancellationToken)
    {
        var existing = await dbContext.UserRoles
            .Include(ur => ur.Role)
            .Where(ur => ur.UserId == userId)
            .ToListAsync(cancellationToken);

        var existingRoleIds = existing.Select(ur => ur.RoleId).ToHashSet();
        var targetRoleIds = roleIds.ToHashSet();
        var now = DateTime.UtcNow;
        var changes = new List<RoleAssignmentChange>();

        foreach (var userRole in existing.Where(ur => !targetRoleIds.Contains(ur.RoleId)).ToArray())
        {
            changes.Add(new RoleAssignmentChange(userRole.RoleId, userRole.Role.Name, false));
            dbContext.UserRoles.Remove(userRole);
        }

        var roleNames = await dbContext.Roles
            .AsNoTracking()
            .Where(r => targetRoleIds.Contains(r.Id))
            .ToDictionaryAsync(r => r.Id, r => r.Name, cancellationToken);

        foreach (var roleId in targetRoleIds.Where(id => !existingRoleIds.Contains(id)))
        {
            dbContext.UserRoles.Add(new UserRole
            {
                UserId = userId,
                RoleId = roleId,
                CreatedAt = now,
                CreatedByUserId = actorUserId
            });

            changes.Add(new RoleAssignmentChange(roleId, roleNames[roleId], true));
        }

        return changes;
    }

    public Task SaveChangesAsync(CancellationToken cancellationToken) =>
        dbContext.SaveChangesAsync(cancellationToken);

    private static IReadOnlyList<RoleDto> MapRoles(IEnumerable<UserRole> userRoles) =>
        userRoles
            .Select(ur => ur.Role)
            .OrderBy(r => r.Name)
            .Select(r => new RoleDto(r.Id, r.Name, r.Description, r.IsSystemRole, r.IsActive))
            .ToArray();
}
