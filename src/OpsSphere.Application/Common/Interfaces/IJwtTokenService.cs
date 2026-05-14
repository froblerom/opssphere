using OpsSphere.Application.Features.Auth;

namespace OpsSphere.Application.Common.Interfaces;

public interface IJwtTokenService
{
    JwtTokenResult CreateToken(AuthenticatedUser user);
}
