namespace OpsSphere.Application.Features.UserManagement;

public sealed record UserSummaryDto(
    Guid Id,
    string Email,
    string FirstName,
    string LastName,
    string DisplayName,
    bool IsActive,
    DateTime CreatedAt,
    DateTime? UpdatedAt,
    DateTime? LastLoginAt,
    IReadOnlyList<RoleDto> Roles);

public sealed record UserDetailDto(
    Guid Id,
    string Email,
    string FirstName,
    string LastName,
    string DisplayName,
    bool IsActive,
    DateTime CreatedAt,
    DateTime? UpdatedAt,
    DateTime? LastLoginAt,
    DateTime? DeactivatedAt,
    IReadOnlyList<RoleDto> Roles);

public sealed record RoleDto(Guid Id, string Name, string? Description, bool IsSystemRole, bool IsActive);

public sealed record PermissionDto(Guid Id, string Code, string Name, string? Description, bool IsActive);
