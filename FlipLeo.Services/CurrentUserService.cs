using FlipLeo.Core.Interfaces;

namespace FlipLeo.Services;

/// <summary>
/// Placeholder until the API has real authentication. Once it does, read the user id and
/// username from the incoming token (IHttpContextAccessor) instead.
/// </summary>
public class CurrentUserService : ICurrentUserService
{
    public Guid? GetUserId() => null;

    public string GetUsername() => "system";
}
