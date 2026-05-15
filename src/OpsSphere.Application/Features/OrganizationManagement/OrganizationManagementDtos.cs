namespace OpsSphere.Application.Features.OrganizationManagement;

public sealed record RegionDto(Guid Id, string Code, string Name, bool IsActive, DateTime CreatedAt, DateTime? UpdatedAt);

public sealed record CountryDto(
    Guid Id,
    Guid RegionId,
    string RegionCode,
    string RegionName,
    string Code,
    string Name,
    bool IsActive,
    DateTime CreatedAt,
    DateTime? UpdatedAt);

public sealed record AccountDto(
    Guid Id,
    Guid CountryId,
    string CountryCode,
    string CountryName,
    Guid RegionId,
    string RegionCode,
    string Code,
    string Name,
    string? Description,
    bool IsActive,
    DateTime CreatedAt,
    DateTime? UpdatedAt);

public sealed record CampaignDto(
    Guid Id,
    Guid AccountId,
    string AccountCode,
    string AccountName,
    Guid CountryId,
    string CountryCode,
    string CountryName,
    Guid RegionId,
    string RegionCode,
    string Code,
    string Name,
    string? Description,
    bool IsActive,
    DateTime CreatedAt,
    DateTime? UpdatedAt);

public sealed record UserScopeAssignmentDto(Guid UserId, string Email, string DisplayName, bool IsActive, IReadOnlyList<string> Roles, IReadOnlyList<UserScopeDto> Scopes);

public sealed record UserScopeDto(
    Guid Id,
    string ScopeType,
    Guid? RegionId,
    string? RegionCode,
    string? RegionName,
    Guid? CountryId,
    string? CountryCode,
    string? CountryName,
    Guid? AccountId,
    string? AccountCode,
    string? AccountName,
    Guid? CampaignId,
    string? CampaignCode,
    string? CampaignName,
    bool IsActive,
    DateTime CreatedAt);
