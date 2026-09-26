using System.Security.Claims;
using System.Text;
using FlipLeo.Core.DTOs.Auth;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.JsonWebTokens;
using Microsoft.IdentityModel.Tokens;

namespace FlipLeo.Api.Utilities;

public interface ITokenService
{
    /// <summary>Creates a signed JWT for the user and wraps it in the response the UI expects.</summary>
    AuthResponse CreateAuthResponse(UserProfile user);
}

public class JwtTokenService(IOptions<JwtSettings> options) : ITokenService
{
    private readonly JwtSettings _settings = options.Value;

    public AuthResponse CreateAuthResponse(UserProfile user)
    {
        var expiresAt = DateTime.UtcNow.AddMinutes(_settings.ExpiresInMinutes);

        var descriptor = new SecurityTokenDescriptor
        {
            Issuer = _settings.Issuer,
            Audience = _settings.Audience,
            Expires = expiresAt,
            Subject = new ClaimsIdentity(new[]
            {
                new Claim(JwtClaimNames.Subject, user.Id.ToString()),
                new Claim(JwtClaimNames.Email, user.Email),
                new Claim(JwtClaimNames.Name, user.DisplayName)
            }),
            SigningCredentials = new SigningCredentials(
                new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_settings.SigningKey)),
                SecurityAlgorithms.HmacSha256)
        };

        return new AuthResponse
        {
            Token = new JsonWebTokenHandler().CreateToken(descriptor),
            ExpiresAt = expiresAt,
            User = user
        };
    }
}
