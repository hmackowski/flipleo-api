using FlipLeo.Repository.Interfaces;

namespace FlipLeo.Repository.Entities;

/// <summary>
/// Base class for tables with audit columns (our version of Common.Core's EntityBase at work).
/// </summary>
public abstract class EntityBase : IEntity
{
    public Guid? CreatedBy { get; set; }
    public string CreatedByUsername { get; set; } = null!;
    public DateTime CreatedDate { get; set; }
    public Guid? UpdatedBy { get; set; }
    public string UpdatedByUsername { get; set; } = null!;
    public DateTime UpdatedDate { get; set; }
}
