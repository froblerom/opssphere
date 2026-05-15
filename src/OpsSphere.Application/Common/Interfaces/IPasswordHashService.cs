namespace OpsSphere.Application.Common.Interfaces;

public interface IPasswordHashService
{
    string HashPassword(Guid userId, string email, string displayName, string password);
}
