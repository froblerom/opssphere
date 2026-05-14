using Microsoft.EntityFrameworkCore;
using OpsSphere.Application.Common.Authorization;
using OpsSphere.Application.Common.Interfaces;
using OpsSphere.Domain.Authorization;
using OpsSphere.Infrastructure.Persistence;

namespace OpsSphere.Infrastructure.Authorization;

internal sealed class ScopeAuthorizationService : IScopeAuthorizationService
{
    private readonly ICurrentUserAuthorizationService authorizationService;
    private readonly OpsSphereDbContext dbContext;

    public ScopeAuthorizationService(ICurrentUserAuthorizationService authorizationService, OpsSphereDbContext dbContext)
    {
        this.authorizationService = authorizationService;
        this.dbContext = dbContext;
    }

    public async Task<AuthorizationDecision> CanAccessRegionAsync(Guid regionId, CancellationToken cancellationToken)
    {
        var profile = await authorizationService.GetCurrentUserAuthorizationAsync(cancellationToken);
        if (profile is null) return AuthorizationDecision.Denied(AuthorizationDecision.ReasonCodes.Unauthenticated);
        if (!profile.IsActive) return AuthorizationDecision.Denied(AuthorizationDecision.ReasonCodes.InactiveUser);

        // Admin bypasses scope checks for administrative access
        if (profile.HasRole(Roles.Admin)) return AuthorizationDecision.Allowed();

        foreach (var scope in profile.Scopes)
        {
            if (scope.ScopeType == ScopeTypes.Region && scope.RegionId == regionId)
                return AuthorizationDecision.Allowed();
        }

        return AuthorizationDecision.Denied(AuthorizationDecision.ReasonCodes.ScopeDenied);
    }

    public async Task<AuthorizationDecision> CanAccessCountryAsync(Guid countryId, CancellationToken cancellationToken)
    {
        var profile = await authorizationService.GetCurrentUserAuthorizationAsync(cancellationToken);
        if (profile is null) return AuthorizationDecision.Denied(AuthorizationDecision.ReasonCodes.Unauthenticated);
        if (!profile.IsActive) return AuthorizationDecision.Denied(AuthorizationDecision.ReasonCodes.InactiveUser);

        if (profile.HasRole(Roles.Admin)) return AuthorizationDecision.Allowed();

        // Resolve country's region for hierarchy checks
        var country = await dbContext.Countries
            .AsNoTracking()
            .Select(c => new { c.Id, c.RegionId })
            .FirstOrDefaultAsync(c => c.Id == countryId, cancellationToken);

        if (country is null) return AuthorizationDecision.Denied(AuthorizationDecision.ReasonCodes.ResourceNotFound);

        foreach (var scope in profile.Scopes)
        {
            // Direct country scope
            if (scope.ScopeType == ScopeTypes.Country && scope.CountryId == countryId)
                return AuthorizationDecision.Allowed();

            // Region scope grants access to all countries under that region
            if (scope.ScopeType == ScopeTypes.Region && scope.RegionId == country.RegionId)
                return AuthorizationDecision.Allowed();
        }

        return AuthorizationDecision.Denied(AuthorizationDecision.ReasonCodes.ScopeDenied);
    }

    public async Task<AuthorizationDecision> CanAccessAccountAsync(Guid accountId, CancellationToken cancellationToken)
    {
        var profile = await authorizationService.GetCurrentUserAuthorizationAsync(cancellationToken);
        if (profile is null) return AuthorizationDecision.Denied(AuthorizationDecision.ReasonCodes.Unauthenticated);
        if (!profile.IsActive) return AuthorizationDecision.Denied(AuthorizationDecision.ReasonCodes.InactiveUser);

        if (profile.HasRole(Roles.Admin)) return AuthorizationDecision.Allowed();

        // Resolve account's country and region for hierarchy checks (join through Country)
        var account = await dbContext.Accounts
            .AsNoTracking()
            .Select(a => new { a.Id, a.CountryId, a.Country.RegionId })
            .FirstOrDefaultAsync(a => a.Id == accountId, cancellationToken);

        if (account is null) return AuthorizationDecision.Denied(AuthorizationDecision.ReasonCodes.ResourceNotFound);

        foreach (var scope in profile.Scopes)
        {
            // Direct account scope
            if (scope.ScopeType == ScopeTypes.Account && scope.AccountId == accountId)
                return AuthorizationDecision.Allowed();

            // Country scope grants access to all accounts in that country
            if (scope.ScopeType == ScopeTypes.Country && scope.CountryId == account.CountryId)
                return AuthorizationDecision.Allowed();

            // Region scope grants access to all accounts under that region
            if (scope.ScopeType == ScopeTypes.Region && scope.RegionId == account.RegionId)
                return AuthorizationDecision.Allowed();
        }

        return AuthorizationDecision.Denied(AuthorizationDecision.ReasonCodes.ScopeDenied);
    }

    public async Task<AuthorizationDecision> CanAccessCampaignAsync(Guid campaignId, CancellationToken cancellationToken)
    {
        var profile = await authorizationService.GetCurrentUserAuthorizationAsync(cancellationToken);
        if (profile is null) return AuthorizationDecision.Denied(AuthorizationDecision.ReasonCodes.Unauthenticated);
        if (!profile.IsActive) return AuthorizationDecision.Denied(AuthorizationDecision.ReasonCodes.InactiveUser);

        if (profile.HasRole(Roles.Admin)) return AuthorizationDecision.Allowed();

        // Resolve campaign's account, country, and region for hierarchy checks
        // Campaign has CountryId directly; get RegionId via Country join
        var campaign = await dbContext.Campaigns
            .AsNoTracking()
            .Select(c => new { c.Id, c.AccountId, c.CountryId, c.Country.RegionId })
            .FirstOrDefaultAsync(c => c.Id == campaignId, cancellationToken);

        if (campaign is null) return AuthorizationDecision.Denied(AuthorizationDecision.ReasonCodes.ResourceNotFound);

        foreach (var scope in profile.Scopes)
        {
            // Direct campaign scope
            if (scope.ScopeType == ScopeTypes.Campaign && scope.CampaignId == campaignId)
                return AuthorizationDecision.Allowed();

            // Account scope grants access to all campaigns under that account
            if (scope.ScopeType == ScopeTypes.Account && scope.AccountId == campaign.AccountId)
                return AuthorizationDecision.Allowed();

            // Country scope grants access to all campaigns in that country
            if (scope.ScopeType == ScopeTypes.Country && scope.CountryId == campaign.CountryId)
                return AuthorizationDecision.Allowed();

            // Region scope grants access to all campaigns under that region
            if (scope.ScopeType == ScopeTypes.Region && scope.RegionId == campaign.RegionId)
                return AuthorizationDecision.Allowed();
        }

        return AuthorizationDecision.Denied(AuthorizationDecision.ReasonCodes.ScopeDenied);
    }
}
