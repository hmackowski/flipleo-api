namespace FlipLeo.Core.Interfaces;

/// <summary>
/// Who is making the current request. Used by FlipLeoUnitOfWork to stamp the audit columns
/// (same role as IAuthService in Pivotal.Core at work).
/// </summary>
public interface ICurrentUserService
{
    Guid? GetUserId();

    string GetUsername();
}
