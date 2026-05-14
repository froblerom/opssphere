using OpsSphere.Application.Common.Authorization;
using OpsSphere.Application.Common.Interfaces;

namespace OpsSphere.Infrastructure.Authorization;

internal sealed class CurrentUserAuthorizationService : ICurrentUserAuthorizationService
{
    private readonly ICurrentUserContext currentUserContext;
    private readonly IAuthUserReader authUserReader;

    public CurrentUserAuthorizationService(ICurrentUserContext currentUserContext, IAuthUserReader authUserReader)
    {
        this.currentUserContext = currentUserContext;
        this.authUserReader = authUserReader;
    }

    public async Task<CurrentUserAuthorizationProfile?> GetCurrentUserAuthorizationAsync(CancellationToken cancellationToken)
    {
        if (!currentUserContext.IsAuthenticated || !currentUserContext.UserId.HasValue)
        {
            return null;
        }

        var profile = await authUserReader.GetUserProfileAsync(currentUserContext.UserId.Value, cancellationToken);
        if (profile is null)
        {
            return null;
        }

        return new CurrentUserAuthorizationProfile(
            profile.Id,
            profile.Email,
            profile.IsActive,
            profile.Roles,
            profile.Permissions,
            profile.Scopes);
    }
}
