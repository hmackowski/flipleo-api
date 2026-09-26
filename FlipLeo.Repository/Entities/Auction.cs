using FlipLeo.Repository.Interfaces;

namespace FlipLeo.Repository.Entities;

public class Auction : EntityBase, ISoftDeletable
{
    public int Id { get; set; }

    /// <summary>The UserAccount that owns this auction.</summary>
    public Guid UserId { get; set; }

    public string Name { get; set; } = null!;

    public int AuctionSiteId { get; set; }

    public string Link { get; set; } = null!;

    public string? ImageUrl { get; set; }

    public decimal CurrentPrice { get; set; }

    public DateTime StartTime { get; set; }

    public DateTime EndTime { get; set; }

    public string? Notes { get; set; }

    public bool IsActive { get; set; }

    public virtual LookupAuctionSite AuctionSite { get; set; } = null!;

    public virtual ICollection<FlipRecord> FlipRecords { get; set; } = new List<FlipRecord>();
}
