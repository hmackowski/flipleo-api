using FlipLeo.Repository.Configurations;
using FlipLeo.Repository.Entities;
using Microsoft.EntityFrameworkCore;

namespace FlipLeo.Repository;

public class FlipLeoContext(DbContextOptions<FlipLeoContext> options) : DbContext(options)
{
    public DbSet<Auction> Auction { get; set; } = null!;
    public DbSet<FlipRecord> FlipRecord { get; set; } = null!;
    public DbSet<FlipRecordAddOn> FlipRecordAddOn { get; set; } = null!;
    public DbSet<LookupAuctionSite> LookupAuctionSite { get; set; } = null!;
    public DbSet<UserAccount> UserAccount { get; set; } = null!;

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.ApplyConfiguration(new AuctionConfiguration());
        modelBuilder.ApplyConfiguration(new FlipRecordConfiguration());
        modelBuilder.ApplyConfiguration(new FlipRecordAddOnConfiguration());
        modelBuilder.ApplyConfiguration(new LookupAuctionSiteConfiguration());
        modelBuilder.ApplyConfiguration(new UserAccountConfiguration());
    }
}
