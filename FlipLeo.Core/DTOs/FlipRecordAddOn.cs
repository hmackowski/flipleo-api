using System.ComponentModel.DataAnnotations;

namespace FlipLeo.Core.DTOs;

public class FlipRecordAddOn
{
    public int Id { get; set; }

    public int FlipRecordId { get; set; }

    /// <summary>Optional: the preset this add-on was created from.</summary>
    public int? AddOnPresetId { get; set; }

    [Required, MaxLength(200)]
    public string Name { get; set; } = string.Empty;

    [Range(0, 99999999.99)]
    public decimal Price { get; set; }

    [MaxLength(1000)]
    public string? Link { get; set; }

    [MaxLength(1000), Url(ErrorMessage = "Image link must be a full http(s) URL.")]
    public string? ImageUrl { get; set; }
}
