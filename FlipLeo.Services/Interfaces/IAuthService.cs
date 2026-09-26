using FlipLeo.Core.DTOs.Auth;

namespace FlipLeo.Services.Interfaces;

public interface IAuthService
{
    /// <summary>Creates a new account. Throws ConflictException if the email is taken.</summary>
    Task<UserProfile> Register(RegisterRequest request);

    /// <summary>Checks the email/password. Throws UnauthorizedException if they don't match.</summary>
    Task<UserProfile> Login(LoginRequest request);

    Task<UserProfile> GetUser(Guid userId);

    /// <summary>
    /// Emails a reset link if the email has an account. Never says whether it does
    /// (so this can't be used to find out who has an account).
    /// </summary>
    Task ForgotPassword(ForgotPasswordRequest request);

    /// <summary>Sets a new password from an emailed token. Throws BadRequestException if the link is invalid or expired.</summary>
    Task ResetPassword(ResetPasswordRequest request);
}
