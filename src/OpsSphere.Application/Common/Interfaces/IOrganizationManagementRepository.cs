using OpsSphere.Application.Features.OrganizationManagement;

namespace OpsSphere.Application.Common.Interfaces;

public interface IOrganizationManagementRepository
{
    Task<IReadOnlyList<RegionDto>> GetRegionsAsync(CancellationToken cancellationToken);
    Task<RegionDto?> GetRegionByIdAsync(Guid id, CancellationToken cancellationToken);
    Task<RegionSnapshot?> GetRegionSnapshotAsync(Guid id, CancellationToken cancellationToken);
    Task<bool> RegionCodeExistsAsync(string code, Guid? excludingId, CancellationToken cancellationToken);
    Task AddRegionAsync(OrganizationEntityCreateModel model, CancellationToken cancellationToken);
    Task UpdateRegionAsync(Guid id, string code, string name, CancellationToken cancellationToken);
    Task<bool> HasActiveCountriesAsync(Guid regionId, CancellationToken cancellationToken);
    Task DeactivateRegionAsync(Guid id, CancellationToken cancellationToken);

    Task<IReadOnlyList<CountryDto>> GetCountriesAsync(CancellationToken cancellationToken);
    Task<CountryDto?> GetCountryByIdAsync(Guid id, CancellationToken cancellationToken);
    Task<CountrySnapshot?> GetCountrySnapshotAsync(Guid id, CancellationToken cancellationToken);
    Task<bool> CountryCodeExistsAsync(string code, Guid? excludingId, CancellationToken cancellationToken);
    Task AddCountryAsync(CountryCreateModel model, CancellationToken cancellationToken);
    Task UpdateCountryAsync(Guid id, Guid regionId, string code, string name, CancellationToken cancellationToken);
    Task<bool> HasActiveAccountsOrCampaignsAsync(Guid countryId, CancellationToken cancellationToken);
    Task DeactivateCountryAsync(Guid id, CancellationToken cancellationToken);

    Task<IReadOnlyList<AccountDto>> GetAccountsAsync(CancellationToken cancellationToken);
    Task<AccountDto?> GetAccountByIdAsync(Guid id, CancellationToken cancellationToken);
    Task<AccountSnapshot?> GetAccountSnapshotAsync(Guid id, CancellationToken cancellationToken);
    Task<bool> AccountCodeExistsAsync(string code, Guid? excludingId, CancellationToken cancellationToken);
    Task AddAccountAsync(AccountCreateModel model, CancellationToken cancellationToken);
    Task UpdateAccountAsync(Guid id, Guid countryId, string code, string name, string? description, CancellationToken cancellationToken);
    Task<bool> HasActiveCampaignsAsync(Guid accountId, CancellationToken cancellationToken);
    Task DeactivateAccountAsync(Guid id, CancellationToken cancellationToken);

    Task<IReadOnlyList<CampaignDto>> GetCampaignsAsync(CancellationToken cancellationToken);
    Task<CampaignDto?> GetCampaignByIdAsync(Guid id, CancellationToken cancellationToken);
    Task<CampaignSnapshot?> GetCampaignSnapshotAsync(Guid id, CancellationToken cancellationToken);
    Task<bool> CampaignCodeExistsAsync(string code, Guid? excludingId, CancellationToken cancellationToken);
    Task AddCampaignAsync(CampaignCreateModel model, CancellationToken cancellationToken);
    Task UpdateCampaignAsync(Guid id, Guid accountId, Guid countryId, string code, string name, string? description, CancellationToken cancellationToken);
    Task DeactivateCampaignAsync(Guid id, CancellationToken cancellationToken);

    Task<UserScopeAssignmentDto?> GetUserScopesAsync(Guid userId, CancellationToken cancellationToken);
    Task<UserScopeValidationUser?> GetScopeAssignmentUserAsync(Guid userId, CancellationToken cancellationToken);
    Task<IReadOnlyList<string>> GetMissingOrInactiveScopeTargetsAsync(IReadOnlyCollection<UserScopeUpsertDto> scopes, CancellationToken cancellationToken);
    Task<IReadOnlyList<UserScopeChange>> ReplaceUserScopesAsync(Guid userId, IReadOnlyCollection<UserScopeUpsertDto> scopes, Guid? actorUserId, CancellationToken cancellationToken);

    Task SaveChangesAsync(CancellationToken cancellationToken);
}

public sealed record OrganizationEntityCreateModel(Guid Id, string Code, string Name);
public sealed record CountryCreateModel(Guid Id, Guid RegionId, string Code, string Name);
public sealed record AccountCreateModel(Guid Id, Guid CountryId, string Code, string Name, string? Description);
public sealed record CampaignCreateModel(Guid Id, Guid AccountId, Guid CountryId, string Code, string Name, string? Description);

public sealed record RegionSnapshot(Guid Id, string Code, string Name, bool IsActive);
public sealed record CountrySnapshot(Guid Id, Guid RegionId, string Code, string Name, bool IsActive);
public sealed record AccountSnapshot(Guid Id, Guid CountryId, string Code, string Name, string? Description, bool IsActive);
public sealed record CampaignSnapshot(Guid Id, Guid AccountId, Guid CountryId, string Code, string Name, string? Description, bool IsActive);
public sealed record UserScopeValidationUser(Guid Id, bool IsActive, IReadOnlyList<string> Roles);
public sealed record UserScopeChange(string Action, UserScopeDto Scope);
