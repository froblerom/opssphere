using System.Text.Json;
using OpsSphere.Application.Common.Exceptions;
using OpsSphere.Application.Common.Interfaces;
using OpsSphere.Domain.Authorization;

namespace OpsSphere.Application.Features.OrganizationManagement;

public sealed class GetRegionsQueryHandler(IOrganizationManagementRepository repository)
{
    public Task<IReadOnlyList<RegionDto>> HandleAsync(GetRegionsQuery query, CancellationToken cancellationToken) =>
        repository.GetRegionsAsync(cancellationToken);
}

public sealed class GetRegionByIdQueryHandler(IOrganizationManagementRepository repository)
{
    public async Task<RegionDto> HandleAsync(GetRegionByIdQuery query, CancellationToken cancellationToken) =>
        await repository.GetRegionByIdAsync(query.Id, cancellationToken) ?? throw new NotFoundException("Region", query.Id);
}

public sealed class CreateRegionCommandHandler(IOrganizationManagementRepository repository, IAuditWriter auditWriter)
{
    public async Task<RegionDto> HandleAsync(CreateRegionCommand command, CancellationToken cancellationToken)
    {
        var (code, name) = await OrganizationValidation.NormalizeCodeNameAsync(command.Code, command.Name, "region", null, repository.RegionCodeExistsAsync, cancellationToken);
        var id = Guid.NewGuid();
        await repository.AddRegionAsync(new OrganizationEntityCreateModel(id, code, name), cancellationToken);
        await auditWriter.WriteAsync("RegionCreated", "Region", id, null, OrganizationAuditJson.Serialize(new { code, name, IsActive = true }), cancellationToken);
        await repository.SaveChangesAsync(cancellationToken);
        return await repository.GetRegionByIdAsync(id, cancellationToken) ?? throw new NotFoundException("Region", id);
    }
}

public sealed class UpdateRegionCommandHandler(IOrganizationManagementRepository repository, IAuditWriter auditWriter)
{
    public async Task<RegionDto> HandleAsync(UpdateRegionCommand command, CancellationToken cancellationToken)
    {
        var existing = await repository.GetRegionSnapshotAsync(command.Id, cancellationToken) ?? throw new NotFoundException("Region", command.Id);
        var (code, name) = await OrganizationValidation.NormalizeCodeNameAsync(command.Code, command.Name, "region", command.Id, repository.RegionCodeExistsAsync, cancellationToken);
        await repository.UpdateRegionAsync(command.Id, code, name, cancellationToken);
        await auditWriter.WriteAsync("RegionUpdated", "Region", command.Id, OrganizationAuditJson.Serialize(existing), OrganizationAuditJson.Serialize(new { code, name, existing.IsActive }), cancellationToken);
        await repository.SaveChangesAsync(cancellationToken);
        return await repository.GetRegionByIdAsync(command.Id, cancellationToken) ?? throw new NotFoundException("Region", command.Id);
    }
}

public sealed class DeactivateRegionCommandHandler(IOrganizationManagementRepository repository, IAuditWriter auditWriter)
{
    public async Task HandleAsync(DeactivateRegionCommand command, CancellationToken cancellationToken)
    {
        var existing = await repository.GetRegionSnapshotAsync(command.Id, cancellationToken) ?? throw new NotFoundException("Region", command.Id);
        if (!existing.IsActive) return;
        if (await repository.HasActiveCountriesAsync(command.Id, cancellationToken)) throw new BusinessRuleException("Region cannot be deactivated while active countries exist.");
        await repository.DeactivateRegionAsync(command.Id, cancellationToken);
        await auditWriter.WriteAsync("RegionDeactivated", "Region", command.Id, OrganizationAuditJson.Serialize(new { existing.IsActive }), OrganizationAuditJson.Serialize(new { IsActive = false }), cancellationToken);
        await repository.SaveChangesAsync(cancellationToken);
    }
}

public sealed class GetCountriesQueryHandler(IOrganizationManagementRepository repository)
{
    public Task<IReadOnlyList<CountryDto>> HandleAsync(GetCountriesQuery query, CancellationToken cancellationToken) => repository.GetCountriesAsync(cancellationToken);
}

public sealed class GetCountryByIdQueryHandler(IOrganizationManagementRepository repository)
{
    public async Task<CountryDto> HandleAsync(GetCountryByIdQuery query, CancellationToken cancellationToken) =>
        await repository.GetCountryByIdAsync(query.Id, cancellationToken) ?? throw new NotFoundException("Country", query.Id);
}

public sealed class CreateCountryCommandHandler(IOrganizationManagementRepository repository, IAuditWriter auditWriter)
{
    public async Task<CountryDto> HandleAsync(CreateCountryCommand command, CancellationToken cancellationToken)
    {
        var parent = await repository.GetRegionSnapshotAsync(command.RegionId, cancellationToken) ?? throw new ValidationException("regionId", "Region must exist.");
        if (!parent.IsActive) throw new ValidationException("regionId", "Country parent region must be active.");
        var (code, name) = await OrganizationValidation.NormalizeCodeNameAsync(command.Code, command.Name, "country", null, repository.CountryCodeExistsAsync, cancellationToken);
        var id = Guid.NewGuid();
        await repository.AddCountryAsync(new CountryCreateModel(id, command.RegionId, code, name), cancellationToken);
        await auditWriter.WriteAsync("CountryCreated", "Country", id, null, OrganizationAuditJson.Serialize(new { command.RegionId, code, name, IsActive = true }), cancellationToken);
        await repository.SaveChangesAsync(cancellationToken);
        return await repository.GetCountryByIdAsync(id, cancellationToken) ?? throw new NotFoundException("Country", id);
    }
}

public sealed class UpdateCountryCommandHandler(IOrganizationManagementRepository repository, IAuditWriter auditWriter)
{
    public async Task<CountryDto> HandleAsync(UpdateCountryCommand command, CancellationToken cancellationToken)
    {
        var existing = await repository.GetCountrySnapshotAsync(command.Id, cancellationToken) ?? throw new NotFoundException("Country", command.Id);
        var parent = await repository.GetRegionSnapshotAsync(command.RegionId, cancellationToken) ?? throw new ValidationException("regionId", "Region must exist.");
        if (!parent.IsActive) throw new ValidationException("regionId", "Country parent region must be active.");
        var (code, name) = await OrganizationValidation.NormalizeCodeNameAsync(command.Code, command.Name, "country", command.Id, repository.CountryCodeExistsAsync, cancellationToken);
        await repository.UpdateCountryAsync(command.Id, command.RegionId, code, name, cancellationToken);
        await auditWriter.WriteAsync("CountryUpdated", "Country", command.Id, OrganizationAuditJson.Serialize(existing), OrganizationAuditJson.Serialize(new { command.RegionId, code, name, existing.IsActive }), cancellationToken);
        await repository.SaveChangesAsync(cancellationToken);
        return await repository.GetCountryByIdAsync(command.Id, cancellationToken) ?? throw new NotFoundException("Country", command.Id);
    }
}

public sealed class DeactivateCountryCommandHandler(IOrganizationManagementRepository repository, IAuditWriter auditWriter)
{
    public async Task HandleAsync(DeactivateCountryCommand command, CancellationToken cancellationToken)
    {
        var existing = await repository.GetCountrySnapshotAsync(command.Id, cancellationToken) ?? throw new NotFoundException("Country", command.Id);
        if (!existing.IsActive) return;
        if (await repository.HasActiveAccountsOrCampaignsAsync(command.Id, cancellationToken)) throw new BusinessRuleException("Country cannot be deactivated while active accounts or campaigns exist.");
        await repository.DeactivateCountryAsync(command.Id, cancellationToken);
        await auditWriter.WriteAsync("CountryDeactivated", "Country", command.Id, OrganizationAuditJson.Serialize(new { existing.IsActive }), OrganizationAuditJson.Serialize(new { IsActive = false }), cancellationToken);
        await repository.SaveChangesAsync(cancellationToken);
    }
}

public sealed class GetAccountsQueryHandler(IOrganizationManagementRepository repository)
{
    public Task<IReadOnlyList<AccountDto>> HandleAsync(GetAccountsQuery query, CancellationToken cancellationToken) => repository.GetAccountsAsync(cancellationToken);
}

public sealed class GetAccountByIdQueryHandler(IOrganizationManagementRepository repository)
{
    public async Task<AccountDto> HandleAsync(GetAccountByIdQuery query, CancellationToken cancellationToken) =>
        await repository.GetAccountByIdAsync(query.Id, cancellationToken) ?? throw new NotFoundException("Account", query.Id);
}

public sealed class CreateAccountCommandHandler(IOrganizationManagementRepository repository, IAuditWriter auditWriter)
{
    public async Task<AccountDto> HandleAsync(CreateAccountCommand command, CancellationToken cancellationToken)
    {
        var country = await repository.GetCountrySnapshotAsync(command.CountryId, cancellationToken) ?? throw new ValidationException("countryId", "Country must exist.");
        if (!country.IsActive) throw new ValidationException("countryId", "Account parent country must be active.");
        var (code, name) = await OrganizationValidation.NormalizeCodeNameAsync(command.Code, command.Name, "account", null, repository.AccountCodeExistsAsync, cancellationToken);
        var description = OrganizationValidation.NormalizeDescription(command.Description);
        var id = Guid.NewGuid();
        await repository.AddAccountAsync(new AccountCreateModel(id, command.CountryId, code, name, description), cancellationToken);
        await auditWriter.WriteAsync("AccountCreated", "Account", id, null, OrganizationAuditJson.Serialize(new { command.CountryId, code, name, description, IsActive = true }), cancellationToken);
        await repository.SaveChangesAsync(cancellationToken);
        return await repository.GetAccountByIdAsync(id, cancellationToken) ?? throw new NotFoundException("Account", id);
    }
}

public sealed class UpdateAccountCommandHandler(IOrganizationManagementRepository repository, IAuditWriter auditWriter)
{
    public async Task<AccountDto> HandleAsync(UpdateAccountCommand command, CancellationToken cancellationToken)
    {
        var existing = await repository.GetAccountSnapshotAsync(command.Id, cancellationToken) ?? throw new NotFoundException("Account", command.Id);
        var country = await repository.GetCountrySnapshotAsync(command.CountryId, cancellationToken) ?? throw new ValidationException("countryId", "Country must exist.");
        if (!country.IsActive) throw new ValidationException("countryId", "Account parent country must be active.");
        var (code, name) = await OrganizationValidation.NormalizeCodeNameAsync(command.Code, command.Name, "account", command.Id, repository.AccountCodeExistsAsync, cancellationToken);
        var description = OrganizationValidation.NormalizeDescription(command.Description);
        await repository.UpdateAccountAsync(command.Id, command.CountryId, code, name, description, cancellationToken);
        await auditWriter.WriteAsync("AccountUpdated", "Account", command.Id, OrganizationAuditJson.Serialize(existing), OrganizationAuditJson.Serialize(new { command.CountryId, code, name, description, existing.IsActive }), cancellationToken);
        await repository.SaveChangesAsync(cancellationToken);
        return await repository.GetAccountByIdAsync(command.Id, cancellationToken) ?? throw new NotFoundException("Account", command.Id);
    }
}

public sealed class DeactivateAccountCommandHandler(IOrganizationManagementRepository repository, IAuditWriter auditWriter)
{
    public async Task HandleAsync(DeactivateAccountCommand command, CancellationToken cancellationToken)
    {
        var existing = await repository.GetAccountSnapshotAsync(command.Id, cancellationToken) ?? throw new NotFoundException("Account", command.Id);
        if (!existing.IsActive) return;
        if (await repository.HasActiveCampaignsAsync(command.Id, cancellationToken)) throw new BusinessRuleException("Account cannot be deactivated while active campaigns exist.");
        await repository.DeactivateAccountAsync(command.Id, cancellationToken);
        await auditWriter.WriteAsync("AccountDeactivated", "Account", command.Id, OrganizationAuditJson.Serialize(new { existing.IsActive }), OrganizationAuditJson.Serialize(new { IsActive = false }), cancellationToken);
        await repository.SaveChangesAsync(cancellationToken);
    }
}

public sealed class GetCampaignsQueryHandler(IOrganizationManagementRepository repository)
{
    public Task<IReadOnlyList<CampaignDto>> HandleAsync(GetCampaignsQuery query, CancellationToken cancellationToken) => repository.GetCampaignsAsync(cancellationToken);
}

public sealed class GetCampaignByIdQueryHandler(IOrganizationManagementRepository repository)
{
    public async Task<CampaignDto> HandleAsync(GetCampaignByIdQuery query, CancellationToken cancellationToken) =>
        await repository.GetCampaignByIdAsync(query.Id, cancellationToken) ?? throw new NotFoundException("Campaign", query.Id);
}

public sealed class CreateCampaignCommandHandler(IOrganizationManagementRepository repository, IAuditWriter auditWriter)
{
    public async Task<CampaignDto> HandleAsync(CreateCampaignCommand command, CancellationToken cancellationToken)
    {
        var (account, country) = await ValidateCampaignParentsAsync(command.AccountId, command.CountryId, cancellationToken);
        var (code, name) = await OrganizationValidation.NormalizeCodeNameAsync(command.Code, command.Name, "campaign", null, repository.CampaignCodeExistsAsync, cancellationToken);
        var description = OrganizationValidation.NormalizeDescription(command.Description);
        var id = Guid.NewGuid();
        await repository.AddCampaignAsync(new CampaignCreateModel(id, command.AccountId, command.CountryId, code, name, description), cancellationToken);
        await auditWriter.WriteAsync("CampaignCreated", "Campaign", id, null, OrganizationAuditJson.Serialize(new { command.AccountId, command.CountryId, code, name, description, IsActive = true }), cancellationToken);
        await repository.SaveChangesAsync(cancellationToken);
        return await repository.GetCampaignByIdAsync(id, cancellationToken) ?? throw new NotFoundException("Campaign", id);

        async Task<(AccountSnapshot Account, CountrySnapshot Country)> ValidateCampaignParentsAsync(Guid accountId, Guid countryId, CancellationToken ct)
        {
            var account = await repository.GetAccountSnapshotAsync(accountId, ct) ?? throw new ValidationException("accountId", "Account must exist.");
            var country = await repository.GetCountrySnapshotAsync(countryId, ct) ?? throw new ValidationException("countryId", "Country must exist.");
            if (!account.IsActive) throw new ValidationException("accountId", "Campaign parent account must be active.");
            if (!country.IsActive) throw new ValidationException("countryId", "Campaign country must be active.");
            if (account.CountryId != countryId) throw new ValidationException("countryId", "Campaign country must match the account country.");
            return (account, country);
        }
    }
}

public sealed class UpdateCampaignCommandHandler(IOrganizationManagementRepository repository, IAuditWriter auditWriter)
{
    public async Task<CampaignDto> HandleAsync(UpdateCampaignCommand command, CancellationToken cancellationToken)
    {
        var existing = await repository.GetCampaignSnapshotAsync(command.Id, cancellationToken) ?? throw new NotFoundException("Campaign", command.Id);
        var account = await repository.GetAccountSnapshotAsync(command.AccountId, cancellationToken) ?? throw new ValidationException("accountId", "Account must exist.");
        var country = await repository.GetCountrySnapshotAsync(command.CountryId, cancellationToken) ?? throw new ValidationException("countryId", "Country must exist.");
        if (!account.IsActive) throw new ValidationException("accountId", "Campaign parent account must be active.");
        if (!country.IsActive) throw new ValidationException("countryId", "Campaign country must be active.");
        if (account.CountryId != command.CountryId) throw new ValidationException("countryId", "Campaign country must match the account country.");
        var (code, name) = await OrganizationValidation.NormalizeCodeNameAsync(command.Code, command.Name, "campaign", command.Id, repository.CampaignCodeExistsAsync, cancellationToken);
        var description = OrganizationValidation.NormalizeDescription(command.Description);
        await repository.UpdateCampaignAsync(command.Id, command.AccountId, command.CountryId, code, name, description, cancellationToken);
        await auditWriter.WriteAsync("CampaignUpdated", "Campaign", command.Id, OrganizationAuditJson.Serialize(existing), OrganizationAuditJson.Serialize(new { command.AccountId, command.CountryId, code, name, description, existing.IsActive }), cancellationToken);
        await repository.SaveChangesAsync(cancellationToken);
        return await repository.GetCampaignByIdAsync(command.Id, cancellationToken) ?? throw new NotFoundException("Campaign", command.Id);
    }
}

public sealed class DeactivateCampaignCommandHandler(IOrganizationManagementRepository repository, IAuditWriter auditWriter)
{
    public async Task HandleAsync(DeactivateCampaignCommand command, CancellationToken cancellationToken)
    {
        var existing = await repository.GetCampaignSnapshotAsync(command.Id, cancellationToken) ?? throw new NotFoundException("Campaign", command.Id);
        if (!existing.IsActive) return;
        await repository.DeactivateCampaignAsync(command.Id, cancellationToken);
        await auditWriter.WriteAsync("CampaignDeactivated", "Campaign", command.Id, OrganizationAuditJson.Serialize(new { existing.IsActive }), OrganizationAuditJson.Serialize(new { IsActive = false }), cancellationToken);
        await repository.SaveChangesAsync(cancellationToken);
    }
}

public sealed class GetUserScopesQueryHandler(IOrganizationManagementRepository repository)
{
    public async Task<UserScopeAssignmentDto> HandleAsync(GetUserScopesQuery query, CancellationToken cancellationToken) =>
        await repository.GetUserScopesAsync(query.UserId, cancellationToken) ?? throw new NotFoundException("User", query.UserId);
}

public sealed class UpdateUserScopesCommandHandler(IOrganizationManagementRepository repository, IAuditWriter auditWriter, ICurrentUserContext currentUserContext)
{
    public async Task<UserScopeAssignmentDto> HandleAsync(UpdateUserScopesCommand command, CancellationToken cancellationToken)
    {
        if (command.Scopes is null) throw new ValidationException("scopes", "Scopes are required.");
        var scopes = NormalizeScopes(command.Scopes);
        var user = await repository.GetScopeAssignmentUserAsync(command.UserId, cancellationToken) ?? throw new NotFoundException("User", command.UserId);
        if (!user.IsActive) throw new ValidationException("userId", "Scopes can only be assigned to an active user.");
        ValidateRoleEligibility(user.Roles, scopes);
        var missing = await repository.GetMissingOrInactiveScopeTargetsAsync(scopes, cancellationToken);
        if (missing.Count > 0) throw new ValidationException("scopes", "All scope targets must exist and be active.");

        var changes = await repository.ReplaceUserScopesAsync(command.UserId, scopes, currentUserContext.UserId, cancellationToken);
        foreach (var change in changes)
        {
            await auditWriter.WriteAsync(
                change.Action,
                "User",
                command.UserId,
                change.Action == "UserScopeDeactivated" ? OrganizationAuditJson.Serialize(change.Scope) : null,
                change.Action == "UserScopeDeactivated" ? null : OrganizationAuditJson.Serialize(change.Scope),
                cancellationToken);
        }

        await repository.SaveChangesAsync(cancellationToken);
        return await repository.GetUserScopesAsync(command.UserId, cancellationToken) ?? throw new NotFoundException("User", command.UserId);
    }

    private static IReadOnlyList<UserScopeUpsertDto> NormalizeScopes(IReadOnlyCollection<UserScopeUpsertDto> requested)
    {
        var normalized = new List<UserScopeUpsertDto>();
        var keys = new HashSet<string>(StringComparer.Ordinal);
        foreach (var scope in requested)
        {
            var scopeType = scope.ScopeType?.Trim();
            var entityId = scopeType switch
            {
                ScopeTypes.Region => scope.RegionId,
                ScopeTypes.Country => scope.CountryId,
                ScopeTypes.Account => scope.AccountId,
                ScopeTypes.Campaign => scope.CampaignId,
                _ => null
            };

            if (!ScopeTypes.All.Contains(scopeType, StringComparer.Ordinal) || !entityId.HasValue)
            {
                throw new ValidationException("scopes", "Each scope must include a valid scope type and matching entity ID.");
            }

            var key = $"{scopeType}:{entityId.Value}";
            if (keys.Add(key))
            {
                normalized.Add(scopeType switch
                {
                    ScopeTypes.Region => new UserScopeUpsertDto(scopeType, entityId, null, null, null),
                    ScopeTypes.Country => new UserScopeUpsertDto(scopeType, null, entityId, null, null),
                    ScopeTypes.Account => new UserScopeUpsertDto(scopeType, null, null, entityId, null),
                    _ => new UserScopeUpsertDto(scopeType, null, null, null, entityId)
                });
            }
        }

        return normalized;
    }

    private static void ValidateRoleEligibility(IReadOnlyList<string> roles, IReadOnlyList<UserScopeUpsertDto> scopes)
    {
        if (roles.Contains(Roles.Admin, StringComparer.Ordinal) && scopes.Count > 0)
        {
            throw new ValidationException("scopes", "Admin users do not require operational scopes.");
        }

        if (scopes.Count == 0) return;
        if (roles.Count == 0) throw new ValidationException("scopes", "Target user must have an eligible role for scopes.");

        foreach (var scope in scopes)
        {
            var eligible = scope.ScopeType switch
            {
                ScopeTypes.Region => roles.Contains(Roles.OperationsManager, StringComparer.Ordinal) || roles.Contains(Roles.Viewer, StringComparer.Ordinal),
                ScopeTypes.Country => roles.Contains(Roles.Viewer, StringComparer.Ordinal),
                ScopeTypes.Account or ScopeTypes.Campaign => roles.Contains(Roles.Supervisor, StringComparer.Ordinal) || roles.Contains(Roles.Agent, StringComparer.Ordinal) || roles.Contains(Roles.Viewer, StringComparer.Ordinal),
                _ => false
            };

            if (!eligible)
            {
                throw new ValidationException("scopes", "One or more scopes are not eligible for the target user's role.");
            }
        }
    }
}

internal static class OrganizationValidation
{
    public static async Task<(string Code, string Name)> NormalizeCodeNameAsync(
        string? code,
        string? name,
        string entityName,
        Guid? excludingId,
        Func<string, Guid?, CancellationToken, Task<bool>> codeExistsAsync,
        CancellationToken cancellationToken)
    {
        var failures = new List<ValidationFailure>();
        if (string.IsNullOrWhiteSpace(code)) failures.Add(new ValidationFailure("code", "Code is required."));
        else if (code.Trim().Length > 50) failures.Add(new ValidationFailure("code", "Code must be 50 characters or fewer."));
        if (string.IsNullOrWhiteSpace(name)) failures.Add(new ValidationFailure("name", "Name is required."));
        else if (name.Trim().Length > 200) failures.Add(new ValidationFailure("name", "Name must be 200 characters or fewer."));
        if (failures.Count > 0) throw new ValidationException(failures);

        var normalizedCode = code!.Trim().ToUpperInvariant();
        if (await codeExistsAsync(normalizedCode, excludingId, cancellationToken))
        {
            throw new ConflictException($"A {entityName} with this code already exists.");
        }

        return (normalizedCode, name!.Trim());
    }

    public static string? NormalizeDescription(string? description) =>
        string.IsNullOrWhiteSpace(description) ? null : description.Trim();
}

internal static class OrganizationAuditJson
{
    public static string Serialize(object value) => JsonSerializer.Serialize(value, new JsonSerializerOptions(JsonSerializerDefaults.Web));
}
