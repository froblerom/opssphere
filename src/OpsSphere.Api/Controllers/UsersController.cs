using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using OpsSphere.Api.Common;
using OpsSphere.Application.Features.UserManagement;
using OpsSphere.Domain.Authorization;

namespace OpsSphere.Api.Controllers;

[ApiController]
[Route("api")]
[Authorize]
public sealed class UsersController : ControllerBase
{
    private readonly GetUsersQueryHandler getUsersQueryHandler;
    private readonly GetUserByIdQueryHandler getUserByIdQueryHandler;
    private readonly CreateUserCommandHandler createUserCommandHandler;
    private readonly UpdateUserCommandHandler updateUserCommandHandler;
    private readonly DeactivateUserCommandHandler deactivateUserCommandHandler;
    private readonly UpdateUserRolesCommandHandler updateUserRolesCommandHandler;
    private readonly GetRolesQueryHandler getRolesQueryHandler;
    private readonly GetPermissionsQueryHandler getPermissionsQueryHandler;
    private readonly ILogger<UsersController> logger;

    public UsersController(
        GetUsersQueryHandler getUsersQueryHandler,
        GetUserByIdQueryHandler getUserByIdQueryHandler,
        CreateUserCommandHandler createUserCommandHandler,
        UpdateUserCommandHandler updateUserCommandHandler,
        DeactivateUserCommandHandler deactivateUserCommandHandler,
        UpdateUserRolesCommandHandler updateUserRolesCommandHandler,
        GetRolesQueryHandler getRolesQueryHandler,
        GetPermissionsQueryHandler getPermissionsQueryHandler,
        ILogger<UsersController> logger)
    {
        this.getUsersQueryHandler = getUsersQueryHandler;
        this.getUserByIdQueryHandler = getUserByIdQueryHandler;
        this.createUserCommandHandler = createUserCommandHandler;
        this.updateUserCommandHandler = updateUserCommandHandler;
        this.deactivateUserCommandHandler = deactivateUserCommandHandler;
        this.updateUserRolesCommandHandler = updateUserRolesCommandHandler;
        this.getRolesQueryHandler = getRolesQueryHandler;
        this.getPermissionsQueryHandler = getPermissionsQueryHandler;
        this.logger = logger;
    }

    [HttpGet("users")]
    [Authorize(Policy = Permissions.UsersView)]
    public async Task<IActionResult> GetUsers(CancellationToken cancellationToken)
    {
        var users = await getUsersQueryHandler.HandleAsync(new GetUsersQuery(), cancellationToken);
        return Ok(new ApiResponse<IReadOnlyList<UserSummaryDto>>(users));
    }

    [HttpGet("users/{id:guid}")]
    [Authorize(Policy = Permissions.UsersView)]
    public async Task<IActionResult> GetUser(Guid id, CancellationToken cancellationToken)
    {
        var user = await getUserByIdQueryHandler.HandleAsync(new GetUserByIdQuery(id), cancellationToken);
        return Ok(new ApiResponse<UserDetailDto>(user));
    }

    [HttpPost("users")]
    [Authorize(Policy = Permissions.UsersManage)]
    public async Task<IActionResult> CreateUser(CreateUserRequest request, CancellationToken cancellationToken)
    {
        var user = await createUserCommandHandler.HandleAsync(new CreateUserCommand(
            request.Email,
            request.FirstName,
            request.LastName,
            request.DisplayName,
            request.TemporaryPassword), cancellationToken);

        logger.LogInformation(
            "User governance change. Action={Action} TargetUserId={TargetUserId} Email={Email}",
            "UserCreated",
            user.Id,
            user.Email);

        return CreatedAtAction(nameof(GetUser), new { id = user.Id }, new ApiResponse<UserDetailDto>(user));
    }

    [HttpPut("users/{id:guid}")]
    [Authorize(Policy = Permissions.UsersManage)]
    public async Task<IActionResult> UpdateUser(Guid id, UpdateUserRequest request, CancellationToken cancellationToken)
    {
        var user = await updateUserCommandHandler.HandleAsync(new UpdateUserCommand(
            id,
            request.Email,
            request.FirstName,
            request.LastName,
            request.DisplayName), cancellationToken);

        logger.LogInformation(
            "User governance change. Action={Action} TargetUserId={TargetUserId} Email={Email}",
            "UserUpdated",
            user.Id,
            user.Email);

        return Ok(new ApiResponse<UserDetailDto>(user));
    }

    [HttpPost("users/{id:guid}/deactivate")]
    [Authorize(Policy = Permissions.UsersManage)]
    public async Task<IActionResult> DeactivateUser(Guid id, CancellationToken cancellationToken)
    {
        await deactivateUserCommandHandler.HandleAsync(new DeactivateUserCommand(id), cancellationToken);

        logger.LogInformation(
            "User governance change. Action={Action} TargetUserId={TargetUserId}",
            "UserDeactivated",
            id);

        return NoContent();
    }

    [HttpPut("users/{id:guid}/roles")]
    [Authorize(Policy = Permissions.RolesManage)]
    public async Task<IActionResult> UpdateUserRoles(Guid id, UpdateUserRolesRequest request, CancellationToken cancellationToken)
    {
        var user = await updateUserRolesCommandHandler.HandleAsync(new UpdateUserRolesCommand(id, request.RoleIds), cancellationToken);

        logger.LogInformation(
            "User governance change. Action={Action} TargetUserId={TargetUserId} RoleCount={RoleCount}",
            "UserRolesUpdated",
            id,
            user.Roles.Count);

        return Ok(new ApiResponse<UserDetailDto>(user));
    }

    [HttpGet("roles")]
    [Authorize(Policy = Permissions.RolesView)]
    public async Task<IActionResult> GetRoles(CancellationToken cancellationToken)
    {
        var roles = await getRolesQueryHandler.HandleAsync(new GetRolesQuery(), cancellationToken);
        return Ok(new ApiResponse<IReadOnlyList<RoleDto>>(roles));
    }

    [HttpGet("permissions")]
    [Authorize(Policy = Permissions.PermissionsView)]
    public async Task<IActionResult> GetPermissions(CancellationToken cancellationToken)
    {
        var permissions = await getPermissionsQueryHandler.HandleAsync(new GetPermissionsQuery(), cancellationToken);
        return Ok(new ApiResponse<IReadOnlyList<PermissionDto>>(permissions));
    }

    public sealed record CreateUserRequest(
        string? Email,
        string? FirstName,
        string? LastName,
        string? DisplayName,
        string? TemporaryPassword);

    public sealed record UpdateUserRequest(
        string? Email,
        string? FirstName,
        string? LastName,
        string? DisplayName);

    public sealed record UpdateUserRolesRequest(IReadOnlyCollection<Guid>? RoleIds);
}
