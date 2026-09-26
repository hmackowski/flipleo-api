using System.ComponentModel.DataAnnotations;
using FlipLeo.Core.Constants;

namespace FlipLeo.Core.DTOs;

public class FlipRecord
{
    public int Id { get; set; }

    [Required, MaxLength(200)]
    public string ItemName { get; set; } = string.Empty;

    [MaxLength(1000), Url(ErrorMessage = "Image link must be a full http(s) URL.")]
    public string? ImageUrl { get; set; }

    [Range(0, 99999999.99)]
    public decimal BuyPrice { get; set; }

    /// <summary>Required once the flip is Sold. While Listed it can hold the asking price.</summary>
    [Range(0, 99999999.99)]
    public decimal? SellPrice { get; set; }

    /// <summary>The date the item was bought.</summary>
    public DateTime FlipDate { get; set; }

    /// <summary>LookupFlipStatus id (see FlipStatusIds). New flips default to Bought.</summary>
    public int FlipStatusId { get; set; } = FlipStatusIds.Bought;

    /// <summary>Read-only: the status name, filled in when reading.</summary>
    public string? FlipStatusName { get; set; }

    /// <summary>Only kept when the flip is Sold.</summary>
    public DateTime? SoldDate { get; set; }

    public int? AuctionId { get; set; }

    /// <summary>Read-only: sum of the add-on prices.</summary>
    public decimal PartsPrice { get; set; }

    /// <summary>Read-only: SellPrice - BuyPrice - PartsPrice once Sold; null while unsold.</summary>
    public decimal? Profit { get; set; }

    /// <summary>
    /// Add-ons for this flip. On create, they're saved with the flip. On update, this is the full
    /// list: missing existing add-ons are removed, existing ones updated, and ones with Id = 0 added.
    /// </summary>
    public List<FlipRecordAddOn> AddOns { get; set; } = [];
}
