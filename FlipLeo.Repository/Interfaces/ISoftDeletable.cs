namespace FlipLeo.Repository.Interfaces;

/// <summary>
/// When someone deletes an entity that implements this interface, FlipLeoUnitOfWork
/// sets IsActive to false instead of removing the row (same as Pivotal.Repository at work).
/// </summary>
public interface ISoftDeletable
{
    bool IsActive { get; set; }
}
