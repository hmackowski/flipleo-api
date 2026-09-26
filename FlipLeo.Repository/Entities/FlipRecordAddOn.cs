using FlipLeo.Repository.Interfaces;

namespace FlipLeo.Repository.Entities;

public class FlipRecordAddOn : EntityBase, ISoftDeletable
{
    public int Id { get; set; }

    public int FlipRecordId { get; set; }

    /// <summary>Set when this add-on was created from one of the user's presets.</summary>
    public int? AddOnPresetId { get; set; }

    public string Name { get; set; } = null!;

    public decimal Price { get; set; }

    public string? Link { get; set; }

    public string? ImageUrl { get; set; }

    public bool IsActive { get; set; }

    public virtual FlipRecord FlipRecord { get; set; } = null!;

    public virtual AddOnPreset? AddOnPreset { get; set; }
}
