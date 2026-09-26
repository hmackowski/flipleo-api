using System.ComponentModel.DataAnnotations;

namespace FlipLeo.Core.DTOs;

public class Auction
{
    public int Id { get; set; }

    [Required, MaxLength(200)]
    public string Name { get; set; } = string.Empty;

    [Range(1, int.MaxValue, ErrorMessage = "An auction site is required.")]
    public int AuctionSiteId { get; set; }

    /// <summary>Read-only: the site's name, filled in when reading.</summary>
    public string? AuctionSiteName { get; set; }

    [Required, MaxLength(1000)]
    public string Link { get; set; } = string.Empty;

    [MaxLength(1000), Url(ErrorMessage = "Image link must be a full http(s) URL.")]
    public string? ImageUrl { get; set; }

    [Range(0, 99999999.99)]
    public decimal CurrentPrice { get; set; }

    public DateTime StartTime { get; set; }

    public DateTime EndTime { get; set; }

    [MaxLength(1000)]
    public string? Notes { get; set; }
}
