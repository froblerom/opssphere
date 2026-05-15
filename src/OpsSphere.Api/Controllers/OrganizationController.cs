using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using OpsSphere.Api.Common;
using OpsSphere.Application.Features.OrganizationManagement;
using OpsSphere.Domain.Authorization;

namespace OpsSphere.Api.Controllers;

[ApiController]
[Route("api")]
[Authorize]
public sealed class OrganizationController : ControllerBase
{
    private readonly IAuthorizationService authorizationService;
    private readonly ILogger<OrganizationController> logger;
    private readonly GetRegionsQueryHandler getRegions;
    private readonly GetRegionByIdQueryHandler getRegion;
    private readonly CreateRegionCommandHandler createRegion;
    private readonly UpdateRegionCommandHandler updateRegion;
    private readonly DeactivateRegionCommandHandler deactivateRegion;
    private readonly GetCountriesQueryHandler getCountries;
    private readonly GetCountryByIdQueryHandler getCountry;
    private readonly CreateCountryCommandHandler createCountry;
    private readonly UpdateCountryCommandHandler updateCountry;
    private readonly DeactivateCountryCommandHandler deactivateCountry;
    private readonly GetAccountsQueryHandler getAccounts;
    private readonly GetAccountByIdQueryHandler getAccount;
    private readonly CreateAccountCommandHandler createAccount;
    private readonly UpdateAccountCommandHandler updateAccount;
    private readonly DeactivateAccountCommandHandler deactivateAccount;
    private readonly GetCampaignsQueryHandler getCampaigns;
    private readonly GetCampaignByIdQueryHandler getCampaign;
    private readonly CreateCampaignCommandHandler createCampaign;
    private readonly UpdateCampaignCommandHandler updateCampaign;
    private readonly DeactivateCampaignCommandHandler deactivateCampaign;
    private readonly GetUserScopesQueryHandler getUserScopes;
    private readonly UpdateUserScopesCommandHandler updateUserScopes;

    public OrganizationController(
        IAuthorizationService authorizationService,
        ILogger<OrganizationController> logger,
        GetRegionsQueryHandler getRegions,
        GetRegionByIdQueryHandler getRegion,
        CreateRegionCommandHandler createRegion,
        UpdateRegionCommandHandler updateRegion,
        DeactivateRegionCommandHandler deactivateRegion,
        GetCountriesQueryHandler getCountries,
        GetCountryByIdQueryHandler getCountry,
        CreateCountryCommandHandler createCountry,
        UpdateCountryCommandHandler updateCountry,
        DeactivateCountryCommandHandler deactivateCountry,
        GetAccountsQueryHandler getAccounts,
        GetAccountByIdQueryHandler getAccount,
        CreateAccountCommandHandler createAccount,
        UpdateAccountCommandHandler updateAccount,
        DeactivateAccountCommandHandler deactivateAccount,
        GetCampaignsQueryHandler getCampaigns,
        GetCampaignByIdQueryHandler getCampaign,
        CreateCampaignCommandHandler createCampaign,
        UpdateCampaignCommandHandler updateCampaign,
        DeactivateCampaignCommandHandler deactivateCampaign,
        GetUserScopesQueryHandler getUserScopes,
        UpdateUserScopesCommandHandler updateUserScopes)
    {
        this.authorizationService = authorizationService;
        this.logger = logger;
        this.getRegions = getRegions;
        this.getRegion = getRegion;
        this.createRegion = createRegion;
        this.updateRegion = updateRegion;
        this.deactivateRegion = deactivateRegion;
        this.getCountries = getCountries;
        this.getCountry = getCountry;
        this.createCountry = createCountry;
        this.updateCountry = updateCountry;
        this.deactivateCountry = deactivateCountry;
        this.getAccounts = getAccounts;
        this.getAccount = getAccount;
        this.createAccount = createAccount;
        this.updateAccount = updateAccount;
        this.deactivateAccount = deactivateAccount;
        this.getCampaigns = getCampaigns;
        this.getCampaign = getCampaign;
        this.createCampaign = createCampaign;
        this.updateCampaign = updateCampaign;
        this.deactivateCampaign = deactivateCampaign;
        this.getUserScopes = getUserScopes;
        this.updateUserScopes = updateUserScopes;
    }

    [HttpGet("regions")]
    [Authorize(Policy = Permissions.OrganizationView)]
    public async Task<IActionResult> GetRegions(CancellationToken cancellationToken) =>
        Ok(new ApiResponse<IReadOnlyList<RegionDto>>(await getRegions.HandleAsync(new GetRegionsQuery(), cancellationToken)));

    [HttpGet("regions/{id:guid}")]
    [Authorize(Policy = Permissions.OrganizationView)]
    public async Task<IActionResult> GetRegion(Guid id, CancellationToken cancellationToken) =>
        Ok(new ApiResponse<RegionDto>(await getRegion.HandleAsync(new GetRegionByIdQuery(id), cancellationToken)));

    [HttpPost("regions")]
    [Authorize(Policy = Permissions.RegionsManage)]
    public async Task<IActionResult> CreateRegion(OrganizationEntityRequest request, CancellationToken cancellationToken)
    {
        var result = await createRegion.HandleAsync(new CreateRegionCommand(request.Code, request.Name), cancellationToken);
        LogOrganizationChange("RegionCreated", "Region", result.Id, result.Code);
        return CreatedAtAction(nameof(GetRegion), new { id = result.Id }, new ApiResponse<RegionDto>(result));
    }

    [HttpPut("regions/{id:guid}")]
    [Authorize(Policy = Permissions.RegionsManage)]
    public async Task<IActionResult> UpdateRegion(Guid id, OrganizationEntityRequest request, CancellationToken cancellationToken)
    {
        var result = await updateRegion.HandleAsync(new UpdateRegionCommand(id, request.Code, request.Name), cancellationToken);
        LogOrganizationChange("RegionUpdated", "Region", result.Id, result.Code);
        return Ok(new ApiResponse<RegionDto>(result));
    }

    [HttpPost("regions/{id:guid}/deactivate")]
    [Authorize(Policy = Permissions.RegionsManage)]
    public async Task<IActionResult> DeactivateRegion(Guid id, CancellationToken cancellationToken)
    {
        await deactivateRegion.HandleAsync(new DeactivateRegionCommand(id), cancellationToken);
        LogOrganizationChange("RegionDeactivated", "Region", id, null);
        return NoContent();
    }

    [HttpGet("countries")]
    [Authorize(Policy = Permissions.OrganizationView)]
    public async Task<IActionResult> GetCountries(CancellationToken cancellationToken) =>
        Ok(new ApiResponse<IReadOnlyList<CountryDto>>(await getCountries.HandleAsync(new GetCountriesQuery(), cancellationToken)));

    [HttpGet("countries/{id:guid}")]
    [Authorize(Policy = Permissions.OrganizationView)]
    public async Task<IActionResult> GetCountry(Guid id, CancellationToken cancellationToken) =>
        Ok(new ApiResponse<CountryDto>(await getCountry.HandleAsync(new GetCountryByIdQuery(id), cancellationToken)));

    [HttpPost("countries")]
    [Authorize(Policy = Permissions.CountriesManage)]
    public async Task<IActionResult> CreateCountry(CountryRequest request, CancellationToken cancellationToken)
    {
        var result = await createCountry.HandleAsync(new CreateCountryCommand(request.RegionId, request.Code, request.Name), cancellationToken);
        LogOrganizationChange("CountryCreated", "Country", result.Id, result.Code);
        return CreatedAtAction(nameof(GetCountry), new { id = result.Id }, new ApiResponse<CountryDto>(result));
    }

    [HttpPut("countries/{id:guid}")]
    [Authorize(Policy = Permissions.CountriesManage)]
    public async Task<IActionResult> UpdateCountry(Guid id, CountryRequest request, CancellationToken cancellationToken)
    {
        var result = await updateCountry.HandleAsync(new UpdateCountryCommand(id, request.RegionId, request.Code, request.Name), cancellationToken);
        LogOrganizationChange("CountryUpdated", "Country", result.Id, result.Code);
        return Ok(new ApiResponse<CountryDto>(result));
    }

    [HttpPost("countries/{id:guid}/deactivate")]
    [Authorize(Policy = Permissions.CountriesManage)]
    public async Task<IActionResult> DeactivateCountry(Guid id, CancellationToken cancellationToken)
    {
        await deactivateCountry.HandleAsync(new DeactivateCountryCommand(id), cancellationToken);
        LogOrganizationChange("CountryDeactivated", "Country", id, null);
        return NoContent();
    }

    [HttpGet("accounts")]
    [Authorize(Policy = Permissions.OrganizationView)]
    public async Task<IActionResult> GetAccounts(CancellationToken cancellationToken) =>
        Ok(new ApiResponse<IReadOnlyList<AccountDto>>(await getAccounts.HandleAsync(new GetAccountsQuery(), cancellationToken)));

    [HttpGet("accounts/{id:guid}")]
    [Authorize(Policy = Permissions.OrganizationView)]
    public async Task<IActionResult> GetAccount(Guid id, CancellationToken cancellationToken) =>
        Ok(new ApiResponse<AccountDto>(await getAccount.HandleAsync(new GetAccountByIdQuery(id), cancellationToken)));

    [HttpPost("accounts")]
    [Authorize(Policy = Permissions.AccountsManage)]
    public async Task<IActionResult> CreateAccount(AccountRequest request, CancellationToken cancellationToken)
    {
        var result = await createAccount.HandleAsync(new CreateAccountCommand(request.CountryId, request.Code, request.Name, request.Description), cancellationToken);
        LogOrganizationChange("AccountCreated", "Account", result.Id, result.Code);
        return CreatedAtAction(nameof(GetAccount), new { id = result.Id }, new ApiResponse<AccountDto>(result));
    }

    [HttpPut("accounts/{id:guid}")]
    [Authorize(Policy = Permissions.AccountsManage)]
    public async Task<IActionResult> UpdateAccount(Guid id, AccountRequest request, CancellationToken cancellationToken)
    {
        var result = await updateAccount.HandleAsync(new UpdateAccountCommand(id, request.CountryId, request.Code, request.Name, request.Description), cancellationToken);
        LogOrganizationChange("AccountUpdated", "Account", result.Id, result.Code);
        return Ok(new ApiResponse<AccountDto>(result));
    }

    [HttpPost("accounts/{id:guid}/deactivate")]
    [Authorize(Policy = Permissions.AccountsManage)]
    public async Task<IActionResult> DeactivateAccount(Guid id, CancellationToken cancellationToken)
    {
        await deactivateAccount.HandleAsync(new DeactivateAccountCommand(id), cancellationToken);
        LogOrganizationChange("AccountDeactivated", "Account", id, null);
        return NoContent();
    }

    [HttpGet("campaigns")]
    [Authorize(Policy = Permissions.OrganizationView)]
    public async Task<IActionResult> GetCampaigns(CancellationToken cancellationToken) =>
        Ok(new ApiResponse<IReadOnlyList<CampaignDto>>(await getCampaigns.HandleAsync(new GetCampaignsQuery(), cancellationToken)));

    [HttpGet("campaigns/{id:guid}")]
    [Authorize(Policy = Permissions.OrganizationView)]
    public async Task<IActionResult> GetCampaign(Guid id, CancellationToken cancellationToken) =>
        Ok(new ApiResponse<CampaignDto>(await getCampaign.HandleAsync(new GetCampaignByIdQuery(id), cancellationToken)));

    [HttpPost("campaigns")]
    [Authorize(Policy = Permissions.CampaignsManage)]
    public async Task<IActionResult> CreateCampaign(CampaignRequest request, CancellationToken cancellationToken)
    {
        var result = await createCampaign.HandleAsync(new CreateCampaignCommand(request.AccountId, request.CountryId, request.Code, request.Name, request.Description), cancellationToken);
        LogOrganizationChange("CampaignCreated", "Campaign", result.Id, result.Code);
        return CreatedAtAction(nameof(GetCampaign), new { id = result.Id }, new ApiResponse<CampaignDto>(result));
    }

    [HttpPut("campaigns/{id:guid}")]
    [Authorize(Policy = Permissions.CampaignsManage)]
    public async Task<IActionResult> UpdateCampaign(Guid id, CampaignRequest request, CancellationToken cancellationToken)
    {
        var result = await updateCampaign.HandleAsync(new UpdateCampaignCommand(id, request.AccountId, request.CountryId, request.Code, request.Name, request.Description), cancellationToken);
        LogOrganizationChange("CampaignUpdated", "Campaign", result.Id, result.Code);
        return Ok(new ApiResponse<CampaignDto>(result));
    }

    [HttpPost("campaigns/{id:guid}/deactivate")]
    [Authorize(Policy = Permissions.CampaignsManage)]
    public async Task<IActionResult> DeactivateCampaign(Guid id, CancellationToken cancellationToken)
    {
        await deactivateCampaign.HandleAsync(new DeactivateCampaignCommand(id), cancellationToken);
        LogOrganizationChange("CampaignDeactivated", "Campaign", id, null);
        return NoContent();
    }

    [HttpGet("users/{id:guid}/scopes")]
    public async Task<IActionResult> GetUserScopes(Guid id, CancellationToken cancellationToken)
    {
        if (!await IsAuthorizedForAnyAsync([Permissions.ScopesView, Permissions.AssignmentsManage]))
        {
            return Forbid();
        }

        return Ok(new ApiResponse<UserScopeAssignmentDto>(await getUserScopes.HandleAsync(new GetUserScopesQuery(id), cancellationToken)));
    }

    [HttpPut("users/{id:guid}/scopes")]
    public async Task<IActionResult> UpdateUserScopes(Guid id, UpdateUserScopesRequest request, CancellationToken cancellationToken)
    {
        if (!await IsAuthorizedForAnyAsync([Permissions.AssignmentsManage, Permissions.ScopesManage]))
        {
            return Forbid();
        }

        var result = await updateUserScopes.HandleAsync(new UpdateUserScopesCommand(id, request.Scopes), cancellationToken);
        logger.LogInformation("Organization assignment change. Action={Action} TargetUserId={TargetUserId} ScopeCount={ScopeCount}", "UserScopesUpdated", id, result.Scopes.Count);
        return Ok(new ApiResponse<UserScopeAssignmentDto>(result));
    }

    private async Task<bool> IsAuthorizedForAnyAsync(IReadOnlyCollection<string> policies)
    {
        foreach (var policy in policies)
        {
            if ((await authorizationService.AuthorizeAsync(User, policy)).Succeeded)
            {
                return true;
            }
        }

        return false;
    }

    private void LogOrganizationChange(string action, string entityType, Guid entityId, string? code) =>
        logger.LogInformation(
            "Organization governance change. Action={Action} EntityType={EntityType} EntityId={EntityId} Code={Code}",
            action,
            entityType,
            entityId,
            code);

    public sealed record OrganizationEntityRequest(string? Code, string? Name);
    public sealed record CountryRequest(Guid RegionId, string? Code, string? Name);
    public sealed record AccountRequest(Guid CountryId, string? Code, string? Name, string? Description);
    public sealed record CampaignRequest(Guid AccountId, Guid CountryId, string? Code, string? Name, string? Description);
    public sealed record UpdateUserScopesRequest(IReadOnlyCollection<UserScopeUpsertDto>? Scopes);
}
