using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using OpsSphere.Api.Common;
using OpsSphere.Application.Common.Interfaces;
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
    private readonly ICorrelationIdAccessor correlationIdAccessor;

    public AuthController(
        LoginCommandHandler loginCommandHandler,
        GetCurrentUserQueryHandler getCurrentUserQueryHandler,
        ICorrelationIdAccessor correlationIdAccessor)
    {
        this.loginCommandHandler = loginCommandHandler;
        this.getCurrentUserQueryHandler = getCurrentUserQueryHandler;
        this.correlationIdAccessor = correlationIdAccessor;
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
            return Unauthorized(Error(result.Error!.Code, result.Error.Message));
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
            return Unauthorized(Error(result.Error!.Code, result.Error.Message));
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

    private ApiErrorEnvelope Error(string code, string message) =>
        new(new ApiError(code, message, CorrelationId: correlationIdAccessor.CorrelationId));

    public sealed record LoginRequest(string? Email, string? Password);

    public sealed record ProtectedSmokeResponse(string Status);
}
