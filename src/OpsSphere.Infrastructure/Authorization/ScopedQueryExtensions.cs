using OpsSphere.Application.Common.Authorization;
using OpsSphere.Application.Features.Auth;
using OpsSphere.Domain.Authorization;
using OpsSphere.Domain.Entities;

namespace OpsSphere.Infrastructure.Authorization;

/// <summary>
/// Reusable IQueryable extension methods for scope-aware query filtering.
/// Backend authorization is the source of truth — these helpers enforce it at the data layer.
/// </summary>
public static class ScopedQueryExtensions
{
    /// <summary>
    /// Filters regions to only those accessible by the given profile.
    /// Admin sees all. Others see only regions in their scope.
    /// </summary>
    public static IQueryable<Region> ApplyScopeFilter(
        this IQueryable<Region> query,
        CurrentUserAuthorizationProfile profile)
    {
        if (profile.HasRole(Roles.Admin)) return query;

        var regionIds = GetAuthorizedRegionIds(profile.Scopes);
        var countryIds = GetDirectCountryIds(profile.Scopes);
        var accountIds = GetDirectAccountIds(profile.Scopes);
        var campaignIds = GetDirectCampaignIds(profile.Scopes);
        if (regionIds.Count == 0 && countryIds.Count == 0 && accountIds.Count == 0 && campaignIds.Count == 0)
            return query.Where(_ => false);

        return query.Where(r =>
            (regionIds.Count > 0 && regionIds.Contains(r.Id)) ||
            (countryIds.Count > 0 && r.Countries.Any(c => countryIds.Contains(c.Id))) ||
            (accountIds.Count > 0 && r.Countries.Any(c => c.Accounts.Any(a => accountIds.Contains(a.Id)))) ||
            (campaignIds.Count > 0 && r.Countries.Any(c => c.Campaigns.Any(campaign => campaignIds.Contains(campaign.Id)))));
    }

    /// <summary>
    /// Filters countries to only those accessible by the given profile.
    /// Hierarchy: Region -> Country.
    /// </summary>
    public static IQueryable<Country> ApplyScopeFilter(
        this IQueryable<Country> query,
        CurrentUserAuthorizationProfile profile)
    {
        if (profile.HasRole(Roles.Admin)) return query;

        var countryIds = GetDirectCountryIds(profile.Scopes);
        var accountIds = GetDirectAccountIds(profile.Scopes);
        var campaignIds = GetDirectCampaignIds(profile.Scopes);
        var regionIds = GetAuthorizedRegionIds(profile.Scopes);

        if (countryIds.Count == 0 && accountIds.Count == 0 && campaignIds.Count == 0 && regionIds.Count == 0)
            return query.Where(_ => false);

        return query.Where(c =>
            (countryIds.Count > 0 && countryIds.Contains(c.Id)) ||
            (accountIds.Count > 0 && c.Accounts.Any(a => accountIds.Contains(a.Id))) ||
            (campaignIds.Count > 0 && c.Campaigns.Any(campaign => campaignIds.Contains(campaign.Id))) ||
            (regionIds.Count > 0 && regionIds.Contains(c.RegionId)));
    }

    /// <summary>
    /// Filters accounts to only those accessible by the given profile.
    /// Hierarchy: Region → Country → Account.
    /// </summary>
    public static IQueryable<Account> ApplyScopeFilter(
        this IQueryable<Account> query,
        CurrentUserAuthorizationProfile profile)
    {
        if (profile.HasRole(Roles.Admin)) return query;

        var accountIds = GetDirectAccountIds(profile.Scopes);
        var campaignIds = GetDirectCampaignIds(profile.Scopes);
        var countryIds = GetDirectCountryIds(profile.Scopes);
        var regionIds = GetAuthorizedRegionIds(profile.Scopes);

        if (accountIds.Count == 0 && campaignIds.Count == 0 && countryIds.Count == 0 && regionIds.Count == 0)
            return query.Where(_ => false);

        return query.Where(a =>
            (accountIds.Count > 0 && accountIds.Contains(a.Id)) ||
            (campaignIds.Count > 0 && a.Campaigns.Any(c => campaignIds.Contains(c.Id))) ||
            (countryIds.Count > 0 && countryIds.Contains(a.CountryId)) ||
            (regionIds.Count > 0 && regionIds.Contains(a.Country.RegionId)));
    }

    /// <summary>
    /// Filters customers to only those accessible by the given profile.
    /// Hierarchy: Region → Country → Account → Customer (Campaign-scoped users access via parent Account).
    /// </summary>
    public static IQueryable<Customer> ApplyScopeFilter(
        this IQueryable<Customer> query,
        CurrentUserAuthorizationProfile profile)
    {
        if (profile.HasRole(Roles.Admin)) return query;

        var accountIds = GetDirectAccountIds(profile.Scopes);
        var campaignIds = GetDirectCampaignIds(profile.Scopes);
        var countryIds = GetDirectCountryIds(profile.Scopes);
        var regionIds = GetAuthorizedRegionIds(profile.Scopes);

        if (accountIds.Count == 0 && campaignIds.Count == 0 && countryIds.Count == 0 && regionIds.Count == 0)
            return query.Where(_ => false);

        return query.Where(c =>
            (accountIds.Count > 0 && accountIds.Contains(c.AccountId)) ||
            (campaignIds.Count > 0 && c.Account.Campaigns.Any(camp => campaignIds.Contains(camp.Id))) ||
            (countryIds.Count > 0 && countryIds.Contains(c.Account.CountryId)) ||
            (regionIds.Count > 0 && regionIds.Contains(c.Account.Country.RegionId)));
    }

    /// <summary>
    /// Filters campaigns to only those accessible by the given profile.
    /// Hierarchy: Region → Country → Account → Campaign.
    /// </summary>
    public static IQueryable<Campaign> ApplyScopeFilter(
        this IQueryable<Campaign> query,
        CurrentUserAuthorizationProfile profile)
    {
        if (profile.HasRole(Roles.Admin)) return query;

        var campaignIds = GetDirectCampaignIds(profile.Scopes);
        var accountIds = GetDirectAccountIds(profile.Scopes);
        var countryIds = GetDirectCountryIds(profile.Scopes);
        var regionIds = GetAuthorizedRegionIds(profile.Scopes);

        if (campaignIds.Count == 0 && accountIds.Count == 0 && countryIds.Count == 0 && regionIds.Count == 0)
            return query.Where(_ => false);

        return query.Where(c =>
            (campaignIds.Count > 0 && campaignIds.Contains(c.Id)) ||
            (accountIds.Count > 0 && accountIds.Contains(c.AccountId)) ||
            (countryIds.Count > 0 && countryIds.Contains(c.CountryId)) ||
            (regionIds.Count > 0 && regionIds.Contains(c.Country.RegionId)));
    }

    /// <summary>
    /// Filters tickets to only those accessible by the given profile.
    /// Hierarchy: Region → Country → Account → Campaign → Ticket.
    /// Ticket stores RegionId, CountryId, AccountId, and CampaignId directly for efficient filtering.
    /// </summary>
    public static IQueryable<Ticket> ApplyScopeFilter(
        this IQueryable<Ticket> query,
        CurrentUserAuthorizationProfile profile)
    {
        if (profile.HasRole(Roles.Admin)) return query;

        var campaignIds = GetDirectCampaignIds(profile.Scopes);
        var accountIds = GetDirectAccountIds(profile.Scopes);
        var countryIds = GetDirectCountryIds(profile.Scopes);
        var regionIds = GetAuthorizedRegionIds(profile.Scopes);

        if (campaignIds.Count == 0 && accountIds.Count == 0 && countryIds.Count == 0 && regionIds.Count == 0)
            return query.Where(_ => false);

        return query.Where(t =>
            (campaignIds.Count > 0 && campaignIds.Contains(t.CampaignId)) ||
            (accountIds.Count > 0 && accountIds.Contains(t.AccountId)) ||
            (countryIds.Count > 0 && countryIds.Contains(t.CountryId)) ||
            (regionIds.Count > 0 && regionIds.Contains(t.RegionId)));
    }

    private static IReadOnlyList<Guid> GetAuthorizedRegionIds(IReadOnlyList<UserScopeDto> scopes) =>
        scopes.Where(s => s.ScopeType == ScopeTypes.Region && s.RegionId.HasValue)
              .Select(s => s.RegionId!.Value)
              .Distinct()
              .ToArray();

    private static IReadOnlyList<Guid> GetDirectCountryIds(IReadOnlyList<UserScopeDto> scopes) =>
        scopes.Where(s => s.ScopeType == ScopeTypes.Country && s.CountryId.HasValue)
              .Select(s => s.CountryId!.Value)
              .Distinct()
              .ToArray();

    private static IReadOnlyList<Guid> GetDirectAccountIds(IReadOnlyList<UserScopeDto> scopes) =>
        scopes.Where(s => s.ScopeType == ScopeTypes.Account && s.AccountId.HasValue)
              .Select(s => s.AccountId!.Value)
              .Distinct()
              .ToArray();

    private static IReadOnlyList<Guid> GetDirectCampaignIds(IReadOnlyList<UserScopeDto> scopes) =>
        scopes.Where(s => s.ScopeType == ScopeTypes.Campaign && s.CampaignId.HasValue)
              .Select(s => s.CampaignId!.Value)
              .Distinct()
              .ToArray();
}
