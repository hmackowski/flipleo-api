using FlipLeo.Core.DTOs.Auth;

namespace FlipLeo.Services.Interfaces;

public interface IAuthService
{
    /// <summary>Creates a new account. Throws ConflictException if the email is taken.</summary>
    Task<UserProfile> Register(RegisterRequest request);

    /// <summary>Checks the email/password. Throws UnauthorizedException if they don't match.</summary>
    Task<UserProfile> Login(LoginRequest request);

    Task<UserProfile> GetUser(Guid userId);
}
