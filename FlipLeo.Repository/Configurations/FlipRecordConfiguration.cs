using FlipLeo.Repository.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace FlipLeo.Repository.Configurations;

public class FlipRecordConfiguration : IEntityTypeConfiguration<FlipRecord>
{
    public void Configure(EntityTypeBuilder<FlipRecord> builder)
    {
        builder.ToTable("FlipRecord");

        builder.HasKey(e => e.Id);

        builder.HasQueryFilter(e => e.IsActive);

        builder.Property(e => e.ItemName)
            .IsRequired()
            .HasMaxLength(200)
            .IsUnicode(false);

        builder.Property(e => e.BuyPrice)
            .HasColumnType("decimal(10, 2)");

        builder.Property(e => e.SellPrice)
            .HasColumnType("decimal(10, 2)");

        builder.Property(e => e.FlipDate)
            .HasColumnType("date");

        builder.ConfigureAuditColumns();

        builder.HasOne(d => d.Auction).WithMany(p => p.FlipRecords)
            .HasForeignKey(d => d.AuctionId)
            .OnDelete(DeleteBehavior.ClientSetNull)
            .HasConstraintName("FK_FlipRecord_Auction");
    }
}
