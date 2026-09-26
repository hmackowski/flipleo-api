using FlipLeo.Repository.Converters;
using FlipLeo.Repository.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace FlipLeo.Repository.Configurations;

public class AuctionConfiguration : IEntityTypeConfiguration<Auction>
{
    public void Configure(EntityTypeBuilder<Auction> builder)
    {
        builder.ToTable("Auction", t => t.HasCheckConstraint("CK_Auction_EndTime", "EndTime >= StartTime"));

        builder.HasKey(e => e.Id);

        // Soft-deleted rows are hidden from every query automatically
        builder.HasQueryFilter(e => e.IsActive);

        builder.Property(e => e.Name)
            .IsRequired()
            .HasMaxLength(200)
            .IsUnicode(false);

        builder.Property(e => e.Link)
            .IsRequired()
            .HasMaxLength(1000)
            .IsUnicode(false);

        builder.Property(e => e.ImageUrl)
            .HasMaxLength(1000)
            .IsUnicode(false);

        builder.Property(e => e.CurrentPrice)
            .HasColumnType("decimal(10, 2)");

        builder.Property(e => e.StartTime)
            .HasColumnType("datetime")
            .HasConversion<UtcDateTimeConverter>();

        builder.Property(e => e.EndTime)
            .HasColumnType("datetime")
            .HasConversion<UtcDateTimeConverter>();

        builder.Property(e => e.Notes)
            .HasMaxLength(1000)
            .IsUnicode(false);

        builder.ConfigureAuditColumns();

        builder.HasOne<UserAccount>().WithMany()
            .HasForeignKey(d => d.UserId)
            .OnDelete(DeleteBehavior.ClientSetNull)
            .HasConstraintName("FK_Auction_UserAccount");

        builder.HasOne(d => d.AuctionSite).WithMany(p => p.Auctions)
            .HasForeignKey(d => d.AuctionSiteId)
            .OnDelete(DeleteBehavior.ClientSetNull)
            .HasConstraintName("FK_Auction_LookupAuctionSite");
    }
}
