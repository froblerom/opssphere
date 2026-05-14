using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using OpsSphere.Application.Features.Auth;
using OpsSphere.Application.Features.Auth.GetCurrentUser;
using OpsSphere.Application.Features.Auth.Login;

namespace OpsSphere.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public sealed class AuthController : ControllerBase
{
    private readonly LoginCommandHandler loginCommandHandler;
    private readonly GetCurrentUserQueryHandler getCurrentUserQueryHandler;

    public AuthController(
        LoginCommandHandler loginCommandHandler,
        GetCurrentUserQueryHandler getCurrentUserQueryHandler)
    {
        this.loginCommandHandler = loginCommandHandler;
        this.getCurrentUserQueryHandler = getCurrentUserQueryHandler;
    }

    [AllowAnonymous]
    [HttpPost("login")]
    public async Task<IActionResult> Login(LoginRequest request, CancellationToken cancellationToken)
    {
        var result = await loginCommandHandler.HandleAsync(
            new LoginCommand(request.Email ?? string.Empty, request.Password ?? string.Empty),
            cancellationToken);

        if (!result.Succeeded || result.Value is null)
        {
            return Unauthorized(Error(result.Error!));
        }

        return Ok(new ApiResponse<LoginResult>(result.Value));
    }

    [Authorize]
    [HttpGet("me")]
    public async Task<IActionResult> Me(CancellationToken cancellationToken)
    {
        var userId = GetSubjectUserId();
        if (userId is null)
        {
            return Unauthorized(Error(GetCurrentUserQueryHandler.UnauthorizedCode, GetCurrentUserQueryHandler.UnauthorizedMessage));
        }

        var result = await getCurrentUserQueryHandler.HandleAsync(new GetCurrentUserQuery(userId.Value), cancellationToken);
        if (!result.Succeeded || result.Value is null)
        {
            return Unauthorized(Error(result.Error!));
        }

        return Ok(new ApiResponse<CurrentUserProfile>(result.Value));
    }

    [Authorize]
    [HttpGet("protected-smoke")]
    public IActionResult ProtectedSmoke()
    {
        return Ok(new ApiResponse<ProtectedSmokeResponse>(new ProtectedSmokeResponse("ok")));
    }

    private Guid? GetSubjectUserId()
    {
        var subject = User.FindFirstValue(JwtRegisteredClaimNames.Sub)
            ?? User.FindFirstValue(ClaimTypes.NameIdentifier);

        return Guid.TryParse(subject, out var userId) ? userId : null;
    }

    private static ApiErrorResponse Error(AuthFailure failure) =>
        Error(failure.Code, failure.Message);

    private static ApiErrorResponse Error(string code, string message) =>
        new(new ApiError(code, message));

    public sealed record LoginRequest(string? Email, string? Password);

    public sealed record ProtectedSmokeResponse(string Status);

    public sealed record ApiResponse<T>(T Data);

    public sealed record ApiErrorResponse(ApiError Error);

    public sealed record ApiError(string Code, string Message);
}
