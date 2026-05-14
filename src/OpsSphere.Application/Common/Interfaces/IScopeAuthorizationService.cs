using OpsSphere.Application.Common.Authorization;

namespace OpsSphere.Application.Common.Interfaces;

public interface IScopeAuthorizationService
{
    Task<AuthorizationDecision> CanAccessRegionAsync(Guid regionId, CancellationToken cancellationToken);
    Task<AuthorizationDecision> CanAccessCountryAsync(Guid countryId, CancellationToken cancellationToken);
    Task<AuthorizationDecision> CanAccessAccountAsync(Guid accountId, CancellationToken cancellationToken);
    Task<AuthorizationDecision> CanAccessCampaignAsync(Guid campaignId, CancellationToken cancellationToken);
}
