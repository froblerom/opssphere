using OpsSphere.Application.Features.Auth;

namespace OpsSphere.Application.Common.Interfaces;

public interface IAuthUserReader
{
    Task<AuthUserCredentialUser?> FindCredentialUserByEmailAsync(string email, CancellationToken cancellationToken);

    Task<AuthUserProfile?> GetUserProfileAsync(Guid userId, CancellationToken cancellationToken);

    Task UpdateLastLoginAtAsync(Guid userId, DateTime lastLoginAt, CancellationToken cancellationToken);
}
