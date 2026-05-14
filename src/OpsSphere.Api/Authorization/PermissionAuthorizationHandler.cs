using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using OpsSphere.Application.Common.Authorization;
using OpsSphere.Application.Common.Interfaces;

namespace OpsSphere.Api.Authorization;

public sealed class PermissionAuthorizationHandler : AuthorizationHandler<PermissionRequirement>
{
    private readonly IServiceScopeFactory scopeFactory;
    private readonly ILogger<PermissionAuthorizationHandler> logger;
    private readonly IHttpContextAccessor httpContextAccessor;

    public PermissionAuthorizationHandler(
        IServiceScopeFactory scopeFactory,
        ILogger<PermissionAuthorizationHandler> logger,
        IHttpContextAccessor httpContextAccessor)
    {
        this.scopeFactory = scopeFactory;
        this.logger = logger;
        this.httpContextAccessor = httpContextAccessor;
    }

    protected override async Task HandleRequirementAsync(
        AuthorizationHandlerContext context,
        PermissionRequirement requirement)
    {
        if (context.User.Identity?.IsAuthenticated != true)
        {
            // JWT middleware returns 401 for unauthenticated — no action needed here
            return;
        }

        using var scope = scopeFactory.CreateScope();
        var authorizationService = scope.ServiceProvider.GetRequiredService<ICurrentUserAuthorizationService>();

        var profile = await authorizationService.GetCurrentUserAuthorizationAsync(CancellationToken.None);

        if (profile is null || !profile.IsActive)
        {
            LogDenial(profile, requirement.Permission, AuthorizationDecision.ReasonCodes.InactiveUser);
            context.Fail();
            return;
        }

        if (!profile.HasPermission(requirement.Permission))
        {
            LogDenial(profile, requirement.Permission, AuthorizationDecision.ReasonCodes.PermissionMissing);
            context.Fail();
            return;
        }

        context.Succeed(requirement);
    }

    private void LogDenial(CurrentUserAuthorizationProfile? profile, string requiredPermission, string reasonCode)
    {
        var httpContext = httpContextAccessor.HttpContext;

        // Safe structured log — never log passwords, hashes, tokens, secrets, or signing keys
        logger.LogWarning(
            "Authorization denied. UserId={UserId} Roles={Roles} RequiredPermission={RequiredPermission} ReasonCode={ReasonCode} Endpoint={Endpoint} HttpMethod={HttpMethod} CorrelationId={CorrelationId}",
            profile?.UserId.ToString() ?? "unknown",
            profile is not null ? string.Join(",", profile.Roles) : "unknown",
            requiredPermission,
            reasonCode,
            httpContext?.Request.Path.Value ?? "unknown",
            httpContext?.Request.Method ?? "unknown",
            httpContext?.TraceIdentifier ?? "unknown");
    }
}
