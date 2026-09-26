using FlipLeo.Core.Exceptions;
using FlipLeo.Core.Interfaces;

namespace FlipLeo.Api.Utilities;

/// <summary>
/// Reads the logged-in user from the JWT on the current request.
/// Claim names match what JwtTokenService puts in the token ("sub", "email").
/// </summary>
public class CurrentUserService(IHttpContextAccessor httpContextAccessor) : ICurrentUserService
{
    public Guid? GetUserId()
    {
        var subject = httpContextAccessor.HttpContext?.User.FindFirst(JwtClaimNames.Subject)?.Value;
        return Guid.TryParse(subject, out var userId) ? userId : null;
    }

    public Guid GetRequiredUserId() =>
        GetUserId() ?? throw new UnauthorizedException("You must be logged in.");

    public string GetUsername() =>
        httpContextAccessor.HttpContext?.User.FindFirst(JwtClaimNames.Email)?.Value ?? "anonymous";
}
