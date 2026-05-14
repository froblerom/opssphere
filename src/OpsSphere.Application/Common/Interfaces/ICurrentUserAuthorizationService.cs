using OpsSphere.Application.Common.Authorization;

namespace OpsSphere.Application.Common.Interfaces;

public interface ICurrentUserAuthorizationService
{
    Task<CurrentUserAuthorizationProfile?> GetCurrentUserAuthorizationAsync(CancellationToken cancellationToken);
}
