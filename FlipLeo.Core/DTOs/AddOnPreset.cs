using System.ComponentModel.DataAnnotations;

namespace FlipLeo.Core.DTOs;

/// <summary>A user's reusable add-on, shown as a quick button when logging a flip.</summary>
public class AddOnPreset
{
    public int Id { get; set; }

    [Required, MaxLength(200)]
    public string Name { get; set; } = string.Empty;

    [Range(0, 99999999.99)]
    public decimal DefaultPrice { get; set; }

    [MaxLength(1000), Url(ErrorMessage = "Link must be a full http(s) URL.")]
    public string? Link { get; set; }

    [MaxLength(1000), Url(ErrorMessage = "Image link must be a full http(s) URL.")]
    public string? ImageUrl { get; set; }
}
