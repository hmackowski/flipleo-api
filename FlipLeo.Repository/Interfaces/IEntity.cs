namespace FlipLeo.Repository.Interfaces;

/// <summary>
/// Entities with audit columns. FlipLeoUnitOfWork fills these in automatically on commit.
/// </summary>
public interface IEntity
{
    Guid? CreatedBy { get; set; }
    string CreatedByUsername { get; set; }
    DateTime CreatedDate { get; set; }
    Guid? UpdatedBy { get; set; }
    string UpdatedByUsername { get; set; }
    DateTime UpdatedDate { get; set; }
}
