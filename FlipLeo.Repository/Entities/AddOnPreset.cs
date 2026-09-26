using FlipLeo.Repository.Interfaces;

namespace FlipLeo.Repository.Entities;

/// <summary>A user's reusable add-on (e.g. "TMR Joysticks", $12), shown as a quick button on flips.</summary>
public class AddOnPreset : EntityBase, ISoftDeletable
{
    public int Id { get; set; }

    /// <summary>The UserAccount that owns this preset.</summary>
    public Guid UserId { get; set; }

    public string Name { get; set; } = null!;

    public decimal DefaultPrice { get; set; }

    public string? Link { get; set; }

    public string? ImageUrl { get; set; }

    public bool IsActive { get; set; }
}
