namespace FlipLeo.Core.Interfaces;

/// <summary>
/// Who is making the current request, read from their JWT. Used by services to scope data
/// to the user, and by FlipLeoUnitOfWork to stamp the audit columns
/// (same role as IAuthService in Pivotal.Core at work).
/// </summary>
public interface ICurrentUserService
{
    /// <summary>The logged-in user's id, or null for anonymous requests.</summary>
    Guid? GetUserId();

    /// <summary>The logged-in user's id; throws UnauthorizedException if nobody is logged in.</summary>
    Guid GetRequiredUserId();

    /// <summary>Used for the *ByUsername audit columns.</summary>
    string GetUsername();
}
