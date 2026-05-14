namespace OpsSphere.Application.Features.Auth;

public sealed record AuthenticatedUser(
    Guid Id,
    string Email,
    string DisplayName,
    IReadOnlyList<string> Roles);

public sealed record AuthUserCredentialUser(
    Guid Id,
    string Email,
    string PasswordHash,
    string DisplayName,
    bool IsActive,
    IReadOnlyList<string> ActiveRoles);

public sealed record AuthUserProfile(
    Guid Id,
    string Email,
    string DisplayName,
    bool IsActive,
    IReadOnlyList<string> Roles,
    IReadOnlyList<string> Permissions,
    IReadOnlyList<UserScopeDto> Scopes);

public sealed record AuthUserSummary(
    Guid Id,
    string Email,
    string DisplayName,
    IReadOnlyList<string> Roles);

public sealed record CurrentUserProfile(
    Guid Id,
    string Email,
    string DisplayName,
    IReadOnlyList<string> Roles,
    IReadOnlyList<string> Permissions,
    IReadOnlyList<UserScopeDto> Scopes);

public sealed record UserScopeDto(
    string ScopeType,
    Guid? RegionId,
    string? RegionCode,
    Guid? CountryId,
    string? CountryCode,
    Guid? AccountId,
    string? AccountCode,
    Guid? CampaignId,
    string? CampaignCode);

public sealed record JwtTokenResult(string AccessToken, int ExpiresIn);

public sealed record LoginResult(
    string AccessToken,
    string TokenType,
    int ExpiresIn,
    AuthUserSummary User);

public sealed record AuthFailure(string Code, string Message);

public sealed record AuthResult<T>(T? Value, AuthFailure? Error)
{
    public bool Succeeded => Error is null;

    public static AuthResult<T> Success(T value) => new(value, null);

    public static AuthResult<T> Failure(string code, string message) => new(default, new AuthFailure(code, message));
}
