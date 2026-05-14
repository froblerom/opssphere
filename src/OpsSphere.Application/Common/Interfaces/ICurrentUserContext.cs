namespace OpsSphere.Application.Common.Interfaces;

public interface ICurrentUserContext
{
    bool IsAuthenticated { get; }
    Guid? UserId { get; }
    string? Email { get; }
    string? DisplayName { get; }
    IReadOnlyCollection<string> Roles { get; }
}
