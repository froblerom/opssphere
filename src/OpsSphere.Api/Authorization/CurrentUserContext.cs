using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using OpsSphere.Application.Common.Interfaces;

namespace OpsSphere.Api.Authorization;

internal sealed class CurrentUserContext : ICurrentUserContext
{
    private readonly IHttpContextAccessor httpContextAccessor;

    public CurrentUserContext(IHttpContextAccessor httpContextAccessor)
    {
        this.httpContextAccessor = httpContextAccessor;
    }

    private ClaimsPrincipal? Principal => httpContextAccessor.HttpContext?.User;

    public bool IsAuthenticated => Principal?.Identity?.IsAuthenticated ?? false;

    public Guid? UserId
    {
        get
        {
            var sub = Principal?.FindFirstValue(JwtRegisteredClaimNames.Sub)
                ?? Principal?.FindFirstValue(ClaimTypes.NameIdentifier);
            return Guid.TryParse(sub, out var id) ? id : null;
        }
    }

    public string? Email => Principal?.FindFirstValue(JwtRegisteredClaimNames.Email)
        ?? Principal?.FindFirstValue(ClaimTypes.Email);

    public string? DisplayName => Principal?.FindFirstValue("display_name")
        ?? Principal?.FindFirstValue(ClaimTypes.Name);

    public IReadOnlyCollection<string> Roles =>
        Principal?.FindAll(ClaimTypes.Role).Select(c => c.Value).ToArray()
        ?? Array.Empty<string>();
}
