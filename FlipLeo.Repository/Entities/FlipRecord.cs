using FlipLeo.Repository.Interfaces;

namespace FlipLeo.Repository.Entities;

public class FlipRecord : EntityBase, ISoftDeletable
{
    public int Id { get; set; }

    /// <summary>The UserAccount that owns this flip (and its add-ons).</summary>
    public Guid UserId { get; set; }

    public string ItemName { get; set; } = null!;

    public string? ImageUrl { get; set; }

    public decimal BuyPrice { get; set; }

    /// <summary>Null until the item sells (required once FlipStatusId is Sold).</summary>
    public decimal? SellPrice { get; set; }

    /// <summary>The date the item was bought.</summary>
    public DateTime FlipDate { get; set; }

    public int FlipStatusId { get; set; }

    public DateTime? SoldDate { get; set; }

    public int? AuctionId { get; set; }

    public bool IsActive { get; set; }

    public virtual Auction? Auction { get; set; }

    public virtual LookupFlipStatus FlipStatus { get; set; } = null!;

    public virtual ICollection<FlipRecordAddOn> AddOns { get; set; } = new List<FlipRecordAddOn>();
}
