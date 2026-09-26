using FlipLeo.Repository.Interfaces;

namespace FlipLeo.Repository.Entities;

public class FlipRecord : EntityBase, ISoftDeletable
{
    public int Id { get; set; }

    /// <summary>The UserAccount that owns this flip (and its add-ons).</summary>
    public Guid UserId { get; set; }

    public string ItemName { get; set; } = null!;

    public decimal BuyPrice { get; set; }

    public decimal SellPrice { get; set; }

    public DateTime FlipDate { get; set; }

    public int? AuctionId { get; set; }

    public bool IsActive { get; set; }

    public virtual Auction? Auction { get; set; }

    public virtual ICollection<FlipRecordAddOn> AddOns { get; set; } = new List<FlipRecordAddOn>();
}
