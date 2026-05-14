using OpsSphere.Application.Common.Interfaces;

namespace OpsSphere.Application.Features.Auth.Login;

public sealed class LoginCommandHandler
{
    public const string InvalidCredentialsCode = "invalid_credentials";
    public const string InvalidCredentialsMessage = "Invalid email or password.";

    private readonly IAuthUserReader authUserReader;
    private readonly IPasswordVerifier passwordVerifier;
    private readonly IJwtTokenService jwtTokenService;
    private readonly IAuthUnitOfWork unitOfWork;

    public LoginCommandHandler(
        IAuthUserReader authUserReader,
        IPasswordVerifier passwordVerifier,
        IJwtTokenService jwtTokenService,
        IAuthUnitOfWork unitOfWork)
    {
        this.authUserReader = authUserReader;
        this.passwordVerifier = passwordVerifier;
        this.jwtTokenService = jwtTokenService;
        this.unitOfWork = unitOfWork;
    }

    public async Task<AuthResult<LoginResult>> HandleAsync(LoginCommand command, CancellationToken cancellationToken)
    {
        var email = command.Email.Trim();
        if (string.IsNullOrWhiteSpace(email) || string.IsNullOrWhiteSpace(command.Password))
        {
            return InvalidCredentials();
        }

        var user = await authUserReader.FindCredentialUserByEmailAsync(email, cancellationToken);
        if (user is null || !user.IsActive || user.ActiveRoles.Count == 0)
        {
            return InvalidCredentials();
        }

        if (!passwordVerifier.VerifyPassword(user, command.Password))
        {
            return InvalidCredentials();
        }

        var authenticatedUser = new AuthenticatedUser(user.Id, user.Email, user.DisplayName, user.ActiveRoles);
        var token = jwtTokenService.CreateToken(authenticatedUser);

        await authUserReader.UpdateLastLoginAtAsync(user.Id, DateTime.UtcNow, cancellationToken);
        await unitOfWork.SaveChangesAsync(cancellationToken);

        return AuthResult<LoginResult>.Success(new LoginResult(
            token.AccessToken,
            "Bearer",
            token.ExpiresIn,
            new AuthUserSummary(user.Id, user.Email, user.DisplayName, user.ActiveRoles)));
    }

    private static AuthResult<LoginResult> InvalidCredentials() =>
        AuthResult<LoginResult>.Failure(InvalidCredentialsCode, InvalidCredentialsMessage);
}
