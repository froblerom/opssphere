using Microsoft.EntityFrameworkCore;
using OpsSphere.Application.Common.Authorization;
using OpsSphere.Application.Common.Interfaces;
using OpsSphere.Application.Features.OrganizationManagement;
using OpsSphere.Domain.Authorization;
using OpsSphere.Domain.Entities;
using OpsSphere.Infrastructure.Authorization;
using OpsSphere.Infrastructure.Persistence;

namespace OpsSphere.Infrastructure.OrganizationManagement;

internal sealed class OrganizationManagementRepository : IOrganizationManagementRepository
{
    private readonly OpsSphereDbContext dbContext;
    private readonly ICurrentUserAuthorizationService authorizationService;

    public OrganizationManagementRepository(OpsSphereDbContext dbContext, ICurrentUserAuthorizationService authorizationService)
    {
        this.dbContext = dbContext;
        this.authorizationService = authorizationService;
    }

    public async Task<IReadOnlyList<RegionDto>> GetRegionsAsync(CancellationToken cancellationToken) =>
        await ApplyScope(await GetProfileAsync(cancellationToken), dbContext.Regions.AsNoTracking())
            .OrderBy(r => r.Code)
            .Select(r => new RegionDto(r.Id, r.Code, r.Name, r.IsActive, r.CreatedAt, r.UpdatedAt))
            .ToArrayAsync(cancellationToken);

    public async Task<RegionDto?> GetRegionByIdAsync(Guid id, CancellationToken cancellationToken) =>
        await ApplyScope(await GetProfileAsync(cancellationToken), dbContext.Regions.AsNoTracking())
            .Where(r => r.Id == id)
            .Select(r => new RegionDto(r.Id, r.Code, r.Name, r.IsActive, r.CreatedAt, r.UpdatedAt))
            .FirstOrDefaultAsync(cancellationToken);

    public async Task<RegionSnapshot?> GetRegionSnapshotAsync(Guid id, CancellationToken cancellationToken) =>
        await dbContext.Regions.AsNoTracking().Where(r => r.Id == id)
            .Select(r => new RegionSnapshot(r.Id, r.Code, r.Name, r.IsActive))
            .FirstOrDefaultAsync(cancellationToken);

    public Task<bool> RegionCodeExistsAsync(string code, Guid? excludingId, CancellationToken cancellationToken) =>
        dbContext.Regions.AnyAsync(r => r.Code == code && (!excludingId.HasValue || r.Id != excludingId.Value), cancellationToken);

    public Task AddRegionAsync(OrganizationEntityCreateModel model, CancellationToken cancellationToken)
    {
        dbContext.Regions.Add(new Region { Id = model.Id, Code = model.Code, Name = model.Name, IsActive = true });
        return Task.CompletedTask;
    }

    public async Task UpdateRegionAsync(Guid id, string code, string name, CancellationToken cancellationToken)
    {
        var region = await dbContext.Regions.FirstAsync(r => r.Id == id, cancellationToken);
        region.Code = code;
        region.Name = name;
    }

    public Task<bool> HasActiveCountriesAsync(Guid regionId, CancellationToken cancellationToken) =>
        dbContext.Countries.AnyAsync(c => c.RegionId == regionId && c.IsActive, cancellationToken);

    public async Task DeactivateRegionAsync(Guid id, CancellationToken cancellationToken) =>
        (await dbContext.Regions.FirstAsync(r => r.Id == id, cancellationToken)).IsActive = false;

    public async Task<IReadOnlyList<CountryDto>> GetCountriesAsync(CancellationToken cancellationToken) =>
        await ApplyScope(await GetProfileAsync(cancellationToken), dbContext.Countries.AsNoTracking())
            .OrderBy(c => c.Code)
            .Select(c => new CountryDto(c.Id, c.RegionId, c.Region.Code, c.Region.Name, c.Code, c.Name, c.IsActive, c.CreatedAt, c.UpdatedAt))
            .ToArrayAsync(cancellationToken);

    public async Task<CountryDto?> GetCountryByIdAsync(Guid id, CancellationToken cancellationToken) =>
        await ApplyScope(await GetProfileAsync(cancellationToken), dbContext.Countries.AsNoTracking())
            .Where(c => c.Id == id)
            .Select(c => new CountryDto(c.Id, c.RegionId, c.Region.Code, c.Region.Name, c.Code, c.Name, c.IsActive, c.CreatedAt, c.UpdatedAt))
            .FirstOrDefaultAsync(cancellationToken);

    public async Task<CountrySnapshot?> GetCountrySnapshotAsync(Guid id, CancellationToken cancellationToken) =>
        await dbContext.Countries.AsNoTracking().Where(c => c.Id == id)
            .Select(c => new CountrySnapshot(c.Id, c.RegionId, c.Code, c.Name, c.IsActive))
            .FirstOrDefaultAsync(cancellationToken);

    public Task<bool> CountryCodeExistsAsync(string code, Guid? excludingId, CancellationToken cancellationToken) =>
        dbContext.Countries.AnyAsync(c => c.Code == code && (!excludingId.HasValue || c.Id != excludingId.Value), cancellationToken);

    public Task AddCountryAsync(CountryCreateModel model, CancellationToken cancellationToken)
    {
        dbContext.Countries.Add(new Country { Id = model.Id, RegionId = model.RegionId, Code = model.Code, Name = model.Name, IsActive = true });
        return Task.CompletedTask;
    }

    public async Task UpdateCountryAsync(Guid id, Guid regionId, string code, string name, CancellationToken cancellationToken)
    {
        var country = await dbContext.Countries.FirstAsync(c => c.Id == id, cancellationToken);
        country.RegionId = regionId;
        country.Code = code;
        country.Name = name;
    }

    public async Task<bool> HasActiveAccountsOrCampaignsAsync(Guid countryId, CancellationToken cancellationToken) =>
        await dbContext.Accounts.AnyAsync(a => a.CountryId == countryId && a.IsActive, cancellationToken) ||
        await dbContext.Campaigns.AnyAsync(c => c.CountryId == countryId && c.IsActive, cancellationToken);

    public async Task DeactivateCountryAsync(Guid id, CancellationToken cancellationToken) =>
        (await dbContext.Countries.FirstAsync(c => c.Id == id, cancellationToken)).IsActive = false;

    public async Task<IReadOnlyList<AccountDto>> GetAccountsAsync(CancellationToken cancellationToken) =>
        await ApplyScope(await GetProfileAsync(cancellationToken), dbContext.Accounts.AsNoTracking())
            .OrderBy(a => a.Code)
            .Select(a => new AccountDto(a.Id, a.CountryId, a.Country.Code, a.Country.Name, a.Country.RegionId, a.Country.Region.Code, a.Code, a.Name, a.Description, a.IsActive, a.CreatedAt, a.UpdatedAt))
            .ToArrayAsync(cancellationToken);

    public async Task<AccountDto?> GetAccountByIdAsync(Guid id, CancellationToken cancellationToken) =>
        await ApplyScope(await GetProfileAsync(cancellationToken), dbContext.Accounts.AsNoTracking())
            .Where(a => a.Id == id)
            .Select(a => new AccountDto(a.Id, a.CountryId, a.Country.Code, a.Country.Name, a.Country.RegionId, a.Country.Region.Code, a.Code, a.Name, a.Description, a.IsActive, a.CreatedAt, a.UpdatedAt))
            .FirstOrDefaultAsync(cancellationToken);

    public async Task<AccountSnapshot?> GetAccountSnapshotAsync(Guid id, CancellationToken cancellationToken) =>
        await dbContext.Accounts.AsNoTracking().Where(a => a.Id == id)
            .Select(a => new AccountSnapshot(a.Id, a.CountryId, a.Code, a.Name, a.Description, a.IsActive))
            .FirstOrDefaultAsync(cancellationToken);

    public Task<bool> AccountCodeExistsAsync(string code, Guid? excludingId, CancellationToken cancellationToken) =>
        dbContext.Accounts.AnyAsync(a => a.Code == code && (!excludingId.HasValue || a.Id != excludingId.Value), cancellationToken);

    public Task AddAccountAsync(AccountCreateModel model, CancellationToken cancellationToken)
    {
        dbContext.Accounts.Add(new Account { Id = model.Id, CountryId = model.CountryId, Code = model.Code, Name = model.Name, Description = model.Description, IsActive = true });
        return Task.CompletedTask;
    }

    public async Task UpdateAccountAsync(Guid id, Guid countryId, string code, string name, string? description, CancellationToken cancellationToken)
    {
        var account = await dbContext.Accounts.FirstAsync(a => a.Id == id, cancellationToken);
        account.CountryId = countryId;
        account.Code = code;
        account.Name = name;
        account.Description = description;
    }

    public Task<bool> HasActiveCampaignsAsync(Guid accountId, CancellationToken cancellationToken) =>
        dbContext.Campaigns.AnyAsync(c => c.AccountId == accountId && c.IsActive, cancellationToken);

    public async Task DeactivateAccountAsync(Guid id, CancellationToken cancellationToken) =>
        (await dbContext.Accounts.FirstAsync(a => a.Id == id, cancellationToken)).IsActive = false;

    public async Task<IReadOnlyList<CampaignDto>> GetCampaignsAsync(CancellationToken cancellationToken) =>
        await ApplyScope(await GetProfileAsync(cancellationToken), dbContext.Campaigns.AsNoTracking())
            .OrderBy(c => c.Code)
            .Select(c => new CampaignDto(c.Id, c.AccountId, c.Account.Code, c.Account.Name, c.CountryId, c.Country.Code, c.Country.Name, c.Country.RegionId, c.Country.Region.Code, c.Code, c.Name, c.Description, c.IsActive, c.CreatedAt, c.UpdatedAt))
            .ToArrayAsync(cancellationToken);

    public async Task<CampaignDto?> GetCampaignByIdAsync(Guid id, CancellationToken cancellationToken) =>
        await ApplyScope(await GetProfileAsync(cancellationToken), dbContext.Campaigns.AsNoTracking())
            .Where(c => c.Id == id)
            .Select(c => new CampaignDto(c.Id, c.AccountId, c.Account.Code, c.Account.Name, c.CountryId, c.Country.Code, c.Country.Name, c.Country.RegionId, c.Country.Region.Code, c.Code, c.Name, c.Description, c.IsActive, c.CreatedAt, c.UpdatedAt))
            .FirstOrDefaultAsync(cancellationToken);

    public async Task<CampaignSnapshot?> GetCampaignSnapshotAsync(Guid id, CancellationToken cancellationToken) =>
        await dbContext.Campaigns.AsNoTracking().Where(c => c.Id == id)
            .Select(c => new CampaignSnapshot(c.Id, c.AccountId, c.CountryId, c.Code, c.Name, c.Description, c.IsActive))
            .FirstOrDefaultAsync(cancellationToken);

    public Task<bool> CampaignCodeExistsAsync(string code, Guid? excludingId, CancellationToken cancellationToken) =>
        dbContext.Campaigns.AnyAsync(c => c.Code == code && (!excludingId.HasValue || c.Id != excludingId.Value), cancellationToken);

    public Task AddCampaignAsync(CampaignCreateModel model, CancellationToken cancellationToken)
    {
        dbContext.Campaigns.Add(new Campaign { Id = model.Id, AccountId = model.AccountId, CountryId = model.CountryId, Code = model.Code, Name = model.Name, Description = model.Description, IsActive = true });
        return Task.CompletedTask;
    }

    public async Task UpdateCampaignAsync(Guid id, Guid accountId, Guid countryId, string code, string name, string? description, CancellationToken cancellationToken)
    {
        var campaign = await dbContext.Campaigns.FirstAsync(c => c.Id == id, cancellationToken);
        campaign.AccountId = accountId;
        campaign.CountryId = countryId;
        campaign.Code = code;
        campaign.Name = name;
        campaign.Description = description;
    }

    public async Task DeactivateCampaignAsync(Guid id, CancellationToken cancellationToken) =>
        (await dbContext.Campaigns.FirstAsync(c => c.Id == id, cancellationToken)).IsActive = false;

    public async Task<UserScopeAssignmentDto?> GetUserScopesAsync(Guid userId, CancellationToken cancellationToken)
    {
        var user = await dbContext.Users.AsNoTracking()
            .Include(u => u.UserRoles).ThenInclude(ur => ur.Role)
            .Include(u => u.UserScopes).ThenInclude(us => us.Region)
            .Include(u => u.UserScopes).ThenInclude(us => us.Country)
            .Include(u => u.UserScopes).ThenInclude(us => us.Account)
            .Include(u => u.UserScopes).ThenInclude(us => us.Campaign)
            .FirstOrDefaultAsync(u => u.Id == userId, cancellationToken);

        return user is null
            ? null
            : new UserScopeAssignmentDto(
                user.Id,
                user.Email,
                user.DisplayName,
                user.IsActive,
                user.UserRoles.Select(ur => ur.Role.Name).OrderBy(n => n).ToArray(),
                user.UserScopes.OrderBy(us => us.ScopeType).ThenBy(us => us.CreatedAt).Select(MapScope).ToArray());
    }

    public async Task<UserScopeValidationUser?> GetScopeAssignmentUserAsync(Guid userId, CancellationToken cancellationToken) =>
        await dbContext.Users.AsNoTracking()
            .Where(u => u.Id == userId)
            .Select(u => new UserScopeValidationUser(u.Id, u.IsActive, u.UserRoles.Select(ur => ur.Role.Name).ToArray()))
            .FirstOrDefaultAsync(cancellationToken);

    public async Task<IReadOnlyList<string>> GetMissingOrInactiveScopeTargetsAsync(IReadOnlyCollection<UserScopeUpsertDto> scopes, CancellationToken cancellationToken)
    {
        var missing = new List<string>();
        foreach (var scope in scopes)
        {
            var exists = scope.ScopeType switch
            {
                ScopeTypes.Region => await dbContext.Regions.AnyAsync(r => r.Id == scope.RegionId && r.IsActive, cancellationToken),
                ScopeTypes.Country => await dbContext.Countries.AnyAsync(c => c.Id == scope.CountryId && c.IsActive, cancellationToken),
                ScopeTypes.Account => await dbContext.Accounts.AnyAsync(a => a.Id == scope.AccountId && a.IsActive, cancellationToken),
                ScopeTypes.Campaign => await dbContext.Campaigns.AnyAsync(c => c.Id == scope.CampaignId && c.IsActive, cancellationToken),
                _ => false
            };

            if (!exists) missing.Add(scope.ScopeType ?? string.Empty);
        }

        return missing;
    }

    public async Task<IReadOnlyList<UserScopeChange>> ReplaceUserScopesAsync(Guid userId, IReadOnlyCollection<UserScopeUpsertDto> scopes, Guid? actorUserId, CancellationToken cancellationToken)
    {
        var existing = await dbContext.UserScopes
            .Include(us => us.Region)
            .Include(us => us.Country)
            .Include(us => us.Account)
            .Include(us => us.Campaign)
            .Where(us => us.UserId == userId)
            .ToListAsync(cancellationToken);

        var now = DateTime.UtcNow;
        var desiredKeys = scopes.Select(ScopeKey).ToHashSet(StringComparer.Ordinal);
        var changes = new List<UserScopeChange>();

        foreach (var scope in existing.Where(us => us.IsActive && !desiredKeys.Contains(ScopeKey(us))).ToArray())
        {
            scope.IsActive = false;
            changes.Add(new UserScopeChange("UserScopeDeactivated", MapScope(scope)));
        }

        foreach (var desired in scopes)
        {
            var key = ScopeKey(desired);
            var current = existing.FirstOrDefault(us => ScopeKey(us) == key);
            if (current is null)
            {
                current = new UserScope
                {
                    Id = Guid.NewGuid(),
                    UserId = userId,
                    ScopeType = desired.ScopeType!,
                    RegionId = desired.RegionId,
                    CountryId = desired.CountryId,
                    AccountId = desired.AccountId,
                    CampaignId = desired.CampaignId,
                    IsActive = true,
                    CreatedAt = now,
                    CreatedByUserId = actorUserId
                };
                dbContext.UserScopes.Add(current);
                changes.Add(new UserScopeChange("UserScopeAssigned", MapScope(current)));
            }
            else if (!current.IsActive)
            {
                current.IsActive = true;
                changes.Add(new UserScopeChange("UserScopeUpdated", MapScope(current)));
            }
        }

        return changes;
    }

    public Task SaveChangesAsync(CancellationToken cancellationToken) => dbContext.SaveChangesAsync(cancellationToken);

    private async Task<CurrentUserAuthorizationProfile> GetProfileAsync(CancellationToken cancellationToken) =>
        await authorizationService.GetCurrentUserAuthorizationAsync(cancellationToken)
        ?? new CurrentUserAuthorizationProfile(Guid.Empty, string.Empty, false, [], [], []);

    private static IQueryable<Region> ApplyScope(CurrentUserAuthorizationProfile profile, IQueryable<Region> query) => query.ApplyScopeFilter(profile);
    private static IQueryable<Country> ApplyScope(CurrentUserAuthorizationProfile profile, IQueryable<Country> query) => query.ApplyScopeFilter(profile);
    private static IQueryable<Account> ApplyScope(CurrentUserAuthorizationProfile profile, IQueryable<Account> query) => query.ApplyScopeFilter(profile);
    private static IQueryable<Campaign> ApplyScope(CurrentUserAuthorizationProfile profile, IQueryable<Campaign> query) => query.ApplyScopeFilter(profile);

    private static UserScopeDto MapScope(UserScope us) => new(
        us.Id,
        us.ScopeType,
        us.RegionId,
        us.Region == null ? null : us.Region.Code,
        us.Region == null ? null : us.Region.Name,
        us.CountryId,
        us.Country == null ? null : us.Country.Code,
        us.Country == null ? null : us.Country.Name,
        us.AccountId,
        us.Account == null ? null : us.Account.Code,
        us.Account == null ? null : us.Account.Name,
        us.CampaignId,
        us.Campaign == null ? null : us.Campaign.Code,
        us.Campaign == null ? null : us.Campaign.Name,
        us.IsActive,
        us.CreatedAt);

    private static string ScopeKey(UserScopeUpsertDto scope) =>
        $"{scope.ScopeType}:{scope.RegionId ?? scope.CountryId ?? scope.AccountId ?? scope.CampaignId}";

    private static string ScopeKey(UserScope scope) =>
        $"{scope.ScopeType}:{scope.RegionId ?? scope.CountryId ?? scope.AccountId ?? scope.CampaignId}";
}
