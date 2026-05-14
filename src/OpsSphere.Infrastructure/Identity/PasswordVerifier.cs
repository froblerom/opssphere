using Microsoft.AspNetCore.Identity;
using OpsSphere.Application.Common.Interfaces;
using OpsSphere.Application.Features.Auth;
using OpsSphere.Domain.Entities;

namespace OpsSphere.Infrastructure.Identity;

internal sealed class PasswordVerifier : IPasswordVerifier
{
    private readonly IPasswordHasher<User> passwordHasher;

    public PasswordVerifier(IPasswordHasher<User> passwordHasher)
    {
        this.passwordHasher = passwordHasher;
    }

    public bool VerifyPassword(AuthUserCredentialUser user, string password)
    {
        var domainUser = new User
        {
            Id = user.Id,
            Email = user.Email,
            DisplayName = user.DisplayName,
            PasswordHash = user.PasswordHash
        };

        var result = passwordHasher.VerifyHashedPassword(domainUser, user.PasswordHash, password);

        return result is PasswordVerificationResult.Success or PasswordVerificationResult.SuccessRehashNeeded;
    }
}
