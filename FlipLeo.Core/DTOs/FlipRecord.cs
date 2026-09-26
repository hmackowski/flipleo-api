using System.ComponentModel.DataAnnotations;

namespace FlipLeo.Core.DTOs;

public class FlipRecord
{
    public int Id { get; set; }

    [Required, MaxLength(200)]
    public string ItemName { get; set; } = string.Empty;

    [Range(0, 99999999.99)]
    public decimal BuyPrice { get; set; }

    [Range(0, 99999999.99)]
    public decimal SellPrice { get; set; }

    public DateTime FlipDate { get; set; }

    public int? AuctionId { get; set; }

    /// <summary>Read-only: sum of the add-on prices.</summary>
    public decimal PartsPrice { get; set; }

    /// <summary>Read-only: SellPrice - BuyPrice - PartsPrice.</summary>
    public decimal Profit { get; set; }

    /// <summary>
    /// Add-ons for this flip. When creating a flip, any add-ons included here are saved with it.
    /// On update, add-ons are managed through their own endpoints.
    /// </summary>
    public List<FlipRecordAddOn> AddOns { get; set; } = [];
}
