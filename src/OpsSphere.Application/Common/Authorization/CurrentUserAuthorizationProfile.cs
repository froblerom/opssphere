using OpsSphere.Application.Features.Auth;

namespace OpsSphere.Application.Common.Authorization;

public sealed record CurrentUserAuthorizationProfile(
    Guid UserId,
    string Email,
    bool IsActive,
    IReadOnlyList<string> Roles,
    IReadOnlyList<string> Permissions,
    IReadOnlyList<UserScopeDto> Scopes)
{
    public bool HasPermission(string permission) =>
        Permissions.Contains(permission, StringComparer.Ordinal);

    public bool HasRole(string role) =>
        Roles.Contains(role, StringComparer.Ordinal);
}
