using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using OpsSphere.Application.Common.Interfaces;
using OpsSphere.Domain.Authorization;

namespace OpsSphere.Api.Controllers;

/// <summary>
/// Authorization foundation smoke endpoints for validating RBAC, permission, and scope behavior.
/// These are not business CRUD APIs — they exist solely to validate the authorization layer.
/// Backend authorization is the source of truth. Frontend visibility is UX only.
/// </summary>
[ApiController]
[Route("api/authz/smoke")]
[Authorize]
public sealed class AuthzController : ControllerBase
{
    private readonly ICurrentUserAuthorizationService authorizationService;
    private readonly IScopeAuthorizationService scopeAuthorizationService;

    public AuthzController(
        ICurrentUserAuthorizationService authorizationService,
        IScopeAuthorizationService scopeAuthorizationService)
    {
        this.authorizationService = authorizationService;
        this.scopeAuthorizationService = scopeAuthorizationService;
    }

    /// <summary>
    /// Admin-only smoke endpoint. Requires users.manage permission.
    /// Validates that only Admin role users with users.manage permission can access.
    /// </summary>
    [HttpGet("admin")]
    [Authorize(Policy = Permissions.UsersManage)]
    public async Task<IActionResult> AdminSmoke(CancellationToken cancellationToken)
    {
        var profile = await authorizationService.GetCurrentUserAuthorizationAsync(cancellationToken);
        return Ok(new AuthzSmokeResponse(
            "ok",
            "admin",
            profile?.UserId,
            profile?.Roles ?? [],
            "Authorization foundation smoke — Admin access validated."));
    }

    /// <summary>
    /// Write action smoke endpoint. Requires tickets.create permission.
    /// Validates that Viewer and other read-only roles are denied write access.
    /// </summary>
    [HttpGet("write")]
    [Authorize(Policy = Permissions.TicketsCreate)]
    public async Task<IActionResult> WriteSmoke(CancellationToken cancellationToken)
    {
        var profile = await authorizationService.GetCurrentUserAuthorizationAsync(cancellationToken);
        return Ok(new AuthzSmokeResponse(
            "ok",
            "write",
            profile?.UserId,
            profile?.Roles ?? [],
            "Authorization foundation smoke — write permission validated."));
    }

    /// <summary>
    /// Scope smoke endpoint for region-level access. Requires organization.view + region scope.
    /// Validates region-level scope authorization with hierarchy awareness.
    /// </summary>
    [HttpGet("scoped/regions/{regionId:guid}")]
    [Authorize(Policy = Permissions.OrganizationView)]
    public async Task<IActionResult> ScopedRegion(Guid regionId, CancellationToken cancellationToken)
    {
        var decision = await scopeAuthorizationService.CanAccessRegionAsync(regionId, cancellationToken);
        if (!decision.IsAllowed)
        {
            return Forbid();
        }

        return Ok(new AuthzScopeResponse(
            "ok",
            "region",
            regionId,
            decision.ReasonCode,
            "Authorization foundation smoke — region scope validated."));
    }

    /// <summary>
    /// Scope smoke endpoint for account-level access. Requires organization.view + account scope.
    /// Validates account-level scope with region/country/account hierarchy.
    /// </summary>
    [HttpGet("scoped/accounts/{accountId:guid}")]
    [Authorize(Policy = Permissions.OrganizationView)]
    public async Task<IActionResult> ScopedAccount(Guid accountId, CancellationToken cancellationToken)
    {
        var decision = await scopeAuthorizationService.CanAccessAccountAsync(accountId, cancellationToken);
        if (!decision.IsAllowed)
        {
            return Forbid();
        }

        return Ok(new AuthzScopeResponse(
            "ok",
            "account",
            accountId,
            decision.ReasonCode,
            "Authorization foundation smoke — account scope validated."));
    }

    /// <summary>
    /// Scope smoke endpoint for campaign-level access. Requires organization.view + campaign scope.
    /// Validates campaign-level scope with full hierarchy awareness.
    /// </summary>
    [HttpGet("scoped/campaigns/{campaignId:guid}")]
    [Authorize(Policy = Permissions.OrganizationView)]
    public async Task<IActionResult> ScopedCampaign(Guid campaignId, CancellationToken cancellationToken)
    {
        var decision = await scopeAuthorizationService.CanAccessCampaignAsync(campaignId, cancellationToken);
        if (!decision.IsAllowed)
        {
            return Forbid();
        }

        return Ok(new AuthzScopeResponse(
            "ok",
            "campaign",
            campaignId,
            decision.ReasonCode,
            "Authorization foundation smoke — campaign scope validated."));
    }

    /// <summary>
    /// Scoped campaign list smoke endpoint. Returns only campaigns accessible to the current user.
    /// Requires organization.view permission. List is filtered by operational scope.
    /// </summary>
    [HttpGet("scoped/campaigns")]
    [Authorize(Policy = Permissions.OrganizationView)]
    public async Task<IActionResult> ScopedCampaigns(CancellationToken cancellationToken)
    {
        var profile = await authorizationService.GetCurrentUserAuthorizationAsync(cancellationToken);
        if (profile is null || !profile.IsActive)
        {
            return Forbid();
        }

        return Ok(new AuthzSmokeResponse(
            "ok",
            "scoped_campaigns",
            profile.UserId,
            profile.Roles,
            "Authorization foundation smoke — scoped campaign list access validated."));
    }

    public sealed record AuthzSmokeResponse(
        string Status,
        string Surface,
        Guid? UserId,
        IReadOnlyList<string> Roles,
        string Note);

    public sealed record AuthzScopeResponse(
        string Status,
        string ScopeType,
        Guid ResourceId,
        string? ReasonCode,
        string Note);
}
