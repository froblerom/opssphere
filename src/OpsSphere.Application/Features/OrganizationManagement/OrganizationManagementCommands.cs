namespace OpsSphere.Application.Features.OrganizationManagement;

public sealed record GetRegionsQuery;
public sealed record GetRegionByIdQuery(Guid Id);
public sealed record CreateRegionCommand(string? Code, string? Name);
public sealed record UpdateRegionCommand(Guid Id, string? Code, string? Name);
public sealed record DeactivateRegionCommand(Guid Id);

public sealed record GetCountriesQuery;
public sealed record GetCountryByIdQuery(Guid Id);
public sealed record CreateCountryCommand(Guid RegionId, string? Code, string? Name);
public sealed record UpdateCountryCommand(Guid Id, Guid RegionId, string? Code, string? Name);
public sealed record DeactivateCountryCommand(Guid Id);

public sealed record GetAccountsQuery;
public sealed record GetAccountByIdQuery(Guid Id);
public sealed record CreateAccountCommand(Guid CountryId, string? Code, string? Name, string? Description);
public sealed record UpdateAccountCommand(Guid Id, Guid CountryId, string? Code, string? Name, string? Description);
public sealed record DeactivateAccountCommand(Guid Id);

public sealed record GetCampaignsQuery;
public sealed record GetCampaignByIdQuery(Guid Id);
public sealed record CreateCampaignCommand(Guid AccountId, Guid CountryId, string? Code, string? Name, string? Description);
public sealed record UpdateCampaignCommand(Guid Id, Guid AccountId, Guid CountryId, string? Code, string? Name, string? Description);
public sealed record DeactivateCampaignCommand(Guid Id);

public sealed record GetUserScopesQuery(Guid UserId);
public sealed record UpdateUserScopesCommand(Guid UserId, IReadOnlyCollection<UserScopeUpsertDto>? Scopes);

public sealed record UserScopeUpsertDto(
    string? ScopeType,
    Guid? RegionId,
    Guid? CountryId,
    Guid? AccountId,
    Guid? CampaignId);
