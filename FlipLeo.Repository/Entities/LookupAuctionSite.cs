namespace FlipLeo.Repository.Entities;

public class LookupAuctionSite
{
    public int Id { get; set; }

    public string Name { get; set; } = null!;

    public string? WebsiteUrl { get; set; }

    public bool IsActive { get; set; }

    public virtual ICollection<Auction> Auctions { get; set; } = new List<Auction>();
}
