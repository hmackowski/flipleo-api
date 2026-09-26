using System.ComponentModel.DataAnnotations;

namespace FlipLeo.Core.DTOs;

public class FlipRecordAddOn
{
    public int Id { get; set; }

    public int FlipRecordId { get; set; }

    [Required, MaxLength(200)]
    public string Name { get; set; } = string.Empty;

    [Range(0, 99999999.99)]
    public decimal Price { get; set; }

    [MaxLength(1000)]
    public string? Link { get; set; }
}
