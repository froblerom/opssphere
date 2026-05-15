using OpsSphere.Application.Features.UserManagement;

namespace OpsSphere.Application.Common.Interfaces;

public interface IUserManagementRepository
{
    Task<IReadOnlyList<UserSummaryDto>> GetUsersAsync(CancellationToken cancellationToken);
    Task<UserDetailDto?> GetUserByIdAsync(Guid userId, CancellationToken cancellationToken);
    Task<bool> EmailExistsAsync(string email, Guid? excludingUserId, CancellationToken cancellationToken);
    Task AddUserAsync(UserCreateModel user, CancellationToken cancellationToken);
    Task<UserUpdateSnapshot?> GetUserUpdateSnapshotAsync(Guid userId, CancellationToken cancellationToken);
    Task UpdateUserAsync(Guid userId, string email, string firstName, string lastName, string displayName, CancellationToken cancellationToken);
    Task<UserStatusSnapshot?> GetUserStatusSnapshotAsync(Guid userId, CancellationToken cancellationToken);
    Task DeactivateUserAsync(Guid userId, DateTime deactivatedAt, CancellationToken cancellationToken);
    Task<IReadOnlyList<RoleDto>> GetRolesAsync(CancellationToken cancellationToken);
    Task<IReadOnlyList<PermissionDto>> GetPermissionsAsync(CancellationToken cancellationToken);
    Task<IReadOnlyList<Guid>> GetMissingActiveRoleIdsAsync(IReadOnlyCollection<Guid> roleIds, CancellationToken cancellationToken);
    Task<IReadOnlyList<RoleAssignmentChange>> ReplaceUserRolesAsync(Guid userId, IReadOnlyCollection<Guid> roleIds, Guid? actorUserId, CancellationToken cancellationToken);
    Task SaveChangesAsync(CancellationToken cancellationToken);
}

public sealed record UserCreateModel(
    Guid Id,
    string Email,
    string PasswordHash,
    string FirstName,
    string LastName,
    string DisplayName,
    bool IsActive,
    DateTime CreatedAt);

public sealed record UserUpdateSnapshot(
    Guid Id,
    string Email,
    string FirstName,
    string LastName,
    string DisplayName);

public sealed record UserStatusSnapshot(Guid Id, bool IsActive);

public sealed record RoleAssignmentChange(Guid RoleId, string RoleName, bool Assigned);
