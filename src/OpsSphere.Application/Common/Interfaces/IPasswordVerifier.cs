using OpsSphere.Application.Features.Auth;

namespace OpsSphere.Application.Common.Interfaces;

public interface IPasswordVerifier
{
    bool VerifyPassword(AuthUserCredentialUser user, string password);
}
