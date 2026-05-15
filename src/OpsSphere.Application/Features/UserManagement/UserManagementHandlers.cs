using System.Text.Json;
using OpsSphere.Application.Common.Exceptions;
using OpsSphere.Application.Common.Interfaces;

namespace OpsSphere.Application.Features.UserManagement;

public sealed class GetUsersQueryHandler
{
    private readonly IUserManagementRepository repository;

    public GetUsersQueryHandler(IUserManagementRepository repository)
    {
        this.repository = repository;
    }

    public Task<IReadOnlyList<UserSummaryDto>> HandleAsync(GetUsersQuery query, CancellationToken cancellationToken) =>
        repository.GetUsersAsync(cancellationToken);
}

public sealed class GetUserByIdQueryHandler
{
    private readonly IUserManagementRepository repository;

    public GetUserByIdQueryHandler(IUserManagementRepository repository)
    {
        this.repository = repository;
    }

    public async Task<UserDetailDto> HandleAsync(GetUserByIdQuery query, CancellationToken cancellationToken)
    {
        var user = await repository.GetUserByIdAsync(query.Id, cancellationToken);
        return user ?? throw new NotFoundException("User", query.Id);
    }
}

public sealed class CreateUserCommandHandler
{
    private readonly IUserManagementRepository repository;
    private readonly IPasswordHashService passwordHashService;
    private readonly IAuditWriter auditWriter;

    public CreateUserCommandHandler(
        IUserManagementRepository repository,
        IPasswordHashService passwordHashService,
        IAuditWriter auditWriter)
    {
        this.repository = repository;
        this.passwordHashService = passwordHashService;
        this.auditWriter = auditWriter;
    }

    public async Task<UserDetailDto> HandleAsync(CreateUserCommand command, CancellationToken cancellationToken)
    {
        var normalized = await ValidateCreateAsync(command, cancellationToken);
        var userId = Guid.NewGuid();
        var passwordHash = passwordHashService.HashPassword(userId, normalized.Email, normalized.DisplayName, normalized.TemporaryPassword);

        await repository.AddUserAsync(new UserCreateModel(
            userId,
            normalized.Email,
            passwordHash,
            normalized.FirstName,
            normalized.LastName,
            normalized.DisplayName,
            true,
            DateTime.UtcNow), cancellationToken);

        await auditWriter.WriteAsync(
            "UserCreated",
            "User",
            userId,
            null,
            UserManagementAuditJson.Serialize(new { normalized.Email, normalized.FirstName, normalized.LastName, normalized.DisplayName, IsActive = true }),
            cancellationToken);

        await repository.SaveChangesAsync(cancellationToken);

        return await repository.GetUserByIdAsync(userId, cancellationToken)
            ?? throw new NotFoundException("User", userId);
    }

    private async Task<NormalizedCreateUser> ValidateCreateAsync(CreateUserCommand command, CancellationToken cancellationToken)
    {
        var failures = UserManagementValidation.ValidateUserFields(command.Email, command.FirstName, command.LastName, command.DisplayName).ToList();
        if (string.IsNullOrWhiteSpace(command.TemporaryPassword))
        {
            failures.Add(new ValidationFailure("temporaryPassword", "Temporary password is required."));
        }
        else if (command.TemporaryPassword.Length < 12)
        {
            failures.Add(new ValidationFailure("temporaryPassword", "Temporary password must be at least 12 characters."));
        }

        if (failures.Count > 0)
        {
            throw new ValidationException(failures);
        }

        var email = command.Email!.Trim();
        if (await repository.EmailExistsAsync(email, null, cancellationToken))
        {
            throw new ConflictException("A user with this email already exists.");
        }

        var firstName = command.FirstName!.Trim();
        var lastName = command.LastName!.Trim();
        var displayName = string.IsNullOrWhiteSpace(command.DisplayName)
            ? $"{firstName} {lastName}"
            : command.DisplayName!.Trim();

        return new NormalizedCreateUser(email, firstName, lastName, displayName, command.TemporaryPassword!);
    }

    private sealed record NormalizedCreateUser(string Email, string FirstName, string LastName, string DisplayName, string TemporaryPassword);
}

public sealed class UpdateUserCommandHandler
{
    private readonly IUserManagementRepository repository;
    private readonly IAuditWriter auditWriter;

    public UpdateUserCommandHandler(IUserManagementRepository repository, IAuditWriter auditWriter)
    {
        this.repository = repository;
        this.auditWriter = auditWriter;
    }

    public async Task<UserDetailDto> HandleAsync(UpdateUserCommand command, CancellationToken cancellationToken)
    {
        var existing = await repository.GetUserUpdateSnapshotAsync(command.Id, cancellationToken)
            ?? throw new NotFoundException("User", command.Id);

        var failures = UserManagementValidation.ValidateUserFields(command.Email, command.FirstName, command.LastName, command.DisplayName).ToList();
        if (failures.Count > 0)
        {
            throw new ValidationException(failures);
        }

        var email = command.Email!.Trim();
        if (await repository.EmailExistsAsync(email, command.Id, cancellationToken))
        {
            throw new ConflictException("A user with this email already exists.");
        }

        var firstName = command.FirstName!.Trim();
        var lastName = command.LastName!.Trim();
        var displayName = string.IsNullOrWhiteSpace(command.DisplayName)
            ? $"{firstName} {lastName}"
            : command.DisplayName!.Trim();

        await repository.UpdateUserAsync(command.Id, email, firstName, lastName, displayName, cancellationToken);
        await auditWriter.WriteAsync(
            "UserUpdated",
            "User",
            command.Id,
            UserManagementAuditJson.Serialize(new { existing.Email, existing.FirstName, existing.LastName, existing.DisplayName }),
            UserManagementAuditJson.Serialize(new { Email = email, FirstName = firstName, LastName = lastName, DisplayName = displayName }),
            cancellationToken);

        await repository.SaveChangesAsync(cancellationToken);

        return await repository.GetUserByIdAsync(command.Id, cancellationToken)
            ?? throw new NotFoundException("User", command.Id);
    }
}

public sealed class DeactivateUserCommandHandler
{
    private readonly IUserManagementRepository repository;
    private readonly IAuditWriter auditWriter;

    public DeactivateUserCommandHandler(IUserManagementRepository repository, IAuditWriter auditWriter)
    {
        this.repository = repository;
        this.auditWriter = auditWriter;
    }

    public async Task HandleAsync(DeactivateUserCommand command, CancellationToken cancellationToken)
    {
        var existing = await repository.GetUserStatusSnapshotAsync(command.Id, cancellationToken)
            ?? throw new NotFoundException("User", command.Id);

        if (!existing.IsActive)
        {
            return;
        }

        await repository.DeactivateUserAsync(command.Id, DateTime.UtcNow, cancellationToken);
        await auditWriter.WriteAsync(
            "UserDeactivated",
            "User",
            command.Id,
            UserManagementAuditJson.Serialize(new { existing.IsActive }),
            UserManagementAuditJson.Serialize(new { IsActive = false }),
            cancellationToken);

        await repository.SaveChangesAsync(cancellationToken);
    }
}

public sealed class UpdateUserRolesCommandHandler
{
    private readonly IUserManagementRepository repository;
    private readonly IAuditWriter auditWriter;
    private readonly ICurrentUserContext currentUserContext;

    public UpdateUserRolesCommandHandler(
        IUserManagementRepository repository,
        IAuditWriter auditWriter,
        ICurrentUserContext currentUserContext)
    {
        this.repository = repository;
        this.auditWriter = auditWriter;
        this.currentUserContext = currentUserContext;
    }

    public async Task<UserDetailDto> HandleAsync(UpdateUserRolesCommand command, CancellationToken cancellationToken)
    {
        if (command.RoleIds is null)
        {
            throw new ValidationException("roleIds", "Role IDs are required.");
        }

        var distinctRoleIds = command.RoleIds.Distinct().ToArray();
        if (distinctRoleIds.Length == 0)
        {
            throw new ValidationException("roleIds", "At least one role is required.");
        }

        var user = await repository.GetUserStatusSnapshotAsync(command.UserId, cancellationToken)
            ?? throw new NotFoundException("User", command.UserId);

        if (!user.IsActive)
        {
            throw new ValidationException("userId", "Roles can only be assigned to an active user.");
        }

        var missingRoleIds = await repository.GetMissingActiveRoleIdsAsync(distinctRoleIds, cancellationToken);
        if (missingRoleIds.Count > 0)
        {
            throw new ValidationException("roleIds", "All roles must exist and be active.");
        }

        var changes = await repository.ReplaceUserRolesAsync(
            command.UserId,
            distinctRoleIds,
            currentUserContext.UserId,
            cancellationToken);

        foreach (var change in changes)
        {
            await auditWriter.WriteAsync(
                change.Assigned ? "RoleAssigned" : "RoleRemoved",
                "User",
                command.UserId,
                change.Assigned ? null : UserManagementAuditJson.Serialize(new { change.RoleId, change.RoleName }),
                change.Assigned ? UserManagementAuditJson.Serialize(new { change.RoleId, change.RoleName }) : null,
                cancellationToken);
        }

        await repository.SaveChangesAsync(cancellationToken);

        return await repository.GetUserByIdAsync(command.UserId, cancellationToken)
            ?? throw new NotFoundException("User", command.UserId);
    }
}

public sealed class GetRolesQueryHandler
{
    private readonly IUserManagementRepository repository;

    public GetRolesQueryHandler(IUserManagementRepository repository)
    {
        this.repository = repository;
    }

    public Task<IReadOnlyList<RoleDto>> HandleAsync(GetRolesQuery query, CancellationToken cancellationToken) =>
        repository.GetRolesAsync(cancellationToken);
}

public sealed class GetPermissionsQueryHandler
{
    private readonly IUserManagementRepository repository;

    public GetPermissionsQueryHandler(IUserManagementRepository repository)
    {
        this.repository = repository;
    }

    public Task<IReadOnlyList<PermissionDto>> HandleAsync(GetPermissionsQuery query, CancellationToken cancellationToken) =>
        repository.GetPermissionsAsync(cancellationToken);
}

internal static class UserManagementValidation
{
    public static IEnumerable<ValidationFailure> ValidateUserFields(string? email, string? firstName, string? lastName, string? displayName)
    {
        if (string.IsNullOrWhiteSpace(email))
        {
            yield return new ValidationFailure("email", "Email is required.");
        }
        else if (!email.Contains('@', StringComparison.Ordinal) || email.Length > 256)
        {
            yield return new ValidationFailure("email", "Email must be a valid address up to 256 characters.");
        }

        if (string.IsNullOrWhiteSpace(firstName))
        {
            yield return new ValidationFailure("firstName", "First name is required.");
        }
        else if (firstName.Trim().Length > 100)
        {
            yield return new ValidationFailure("firstName", "First name must be 100 characters or fewer.");
        }

        if (string.IsNullOrWhiteSpace(lastName))
        {
            yield return new ValidationFailure("lastName", "Last name is required.");
        }
        else if (lastName.Trim().Length > 100)
        {
            yield return new ValidationFailure("lastName", "Last name must be 100 characters or fewer.");
        }

        if (!string.IsNullOrWhiteSpace(displayName) && displayName.Trim().Length > 200)
        {
            yield return new ValidationFailure("displayName", "Display name must be 200 characters or fewer.");
        }
    }
}

internal static class UserManagementAuditJson
{
    public static string Serialize(object value) => JsonSerializer.Serialize(value, new JsonSerializerOptions(JsonSerializerDefaults.Web));
}
