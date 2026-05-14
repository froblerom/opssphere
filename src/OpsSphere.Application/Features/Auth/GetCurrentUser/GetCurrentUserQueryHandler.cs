using OpsSphere.Application.Common.Interfaces;

namespace OpsSphere.Application.Features.Auth.GetCurrentUser;

public sealed class GetCurrentUserQueryHandler
{
    public const string UnauthorizedCode = "unauthorized";
    public const string UnauthorizedMessage = "Authentication is required.";

    private readonly IAuthUserReader authUserReader;

    public GetCurrentUserQueryHandler(IAuthUserReader authUserReader)
    {
        this.authUserReader = authUserReader;
    }

    public async Task<AuthResult<CurrentUserProfile>> HandleAsync(GetCurrentUserQuery query, CancellationToken cancellationToken)
    {
        var profile = await authUserReader.GetUserProfileAsync(query.UserId, cancellationToken);

        if (profile is null || !profile.IsActive || profile.Roles.Count == 0)
        {
            return AuthResult<CurrentUserProfile>.Failure(UnauthorizedCode, UnauthorizedMessage);
        }

        return AuthResult<CurrentUserProfile>.Success(new CurrentUserProfile(
            profile.Id,
            profile.Email,
            profile.DisplayName,
            profile.Roles,
            profile.Permissions,
            profile.Scopes));
    }
}
