namespace OpsSphere.Application.Features.UserManagement;

public sealed record GetUsersQuery;

public sealed record GetUserByIdQuery(Guid Id);

public sealed record CreateUserCommand(
    string? Email,
    string? FirstName,
    string? LastName,
    string? DisplayName,
    string? TemporaryPassword);

public sealed record UpdateUserCommand(
    Guid Id,
    string? Email,
    string? FirstName,
    string? LastName,
    string? DisplayName);

public sealed record DeactivateUserCommand(Guid Id);

public sealed record UpdateUserRolesCommand(Guid UserId, IReadOnlyCollection<Guid>? RoleIds);

public sealed record GetRolesQuery;

public sealed record GetPermissionsQuery;
