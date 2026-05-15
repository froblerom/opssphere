using Microsoft.AspNetCore.Identity;
using OpsSphere.Application.Common.Interfaces;
using OpsSphere.Domain.Entities;

namespace OpsSphere.Infrastructure.Identity;

internal sealed class PasswordHashService : IPasswordHashService
{
    private readonly IPasswordHasher<User> passwordHasher;

    public PasswordHashService(IPasswordHasher<User> passwordHasher)
    {
        this.passwordHasher = passwordHasher;
    }

    public string HashPassword(Guid userId, string email, string displayName, string password)
    {
        var user = new User
        {
            Id = userId,
            Email = email,
            DisplayName = displayName
        };

        return passwordHasher.HashPassword(user, password);
    }
}
