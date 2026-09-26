using FlipLeo.Repository.Interfaces;

namespace FlipLeo.Repository.Entities;

public class FlipRecordAddOn : EntityBase, ISoftDeletable
{
    public int Id { get; set; }

    public int FlipRecordId { get; set; }

    public string Name { get; set; } = null!;

    public decimal Price { get; set; }

    public string? Link { get; set; }

    public bool IsActive { get; set; }

    public virtual FlipRecord FlipRecord { get; set; } = null!;
}
