using FlipLeo.Repository.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace FlipLeo.Repository.Configurations;

public class FlipRecordAddOnConfiguration : IEntityTypeConfiguration<FlipRecordAddOn>
{
    public void Configure(EntityTypeBuilder<FlipRecordAddOn> builder)
    {
        builder.ToTable("FlipRecordAddOn");

        builder.HasKey(e => e.Id);

        builder.HasQueryFilter(e => e.IsActive);

        builder.Property(e => e.Name)
            .IsRequired()
            .HasMaxLength(200)
            .IsUnicode(false);

        builder.Property(e => e.Price)
            .HasColumnType("decimal(10, 2)");

        builder.Property(e => e.Link)
            .HasMaxLength(1000)
            .IsUnicode(false);

        builder.ConfigureAuditColumns();

        builder.HasOne(d => d.FlipRecord).WithMany(p => p.AddOns)
            .HasForeignKey(d => d.FlipRecordId)
            .OnDelete(DeleteBehavior.ClientSetNull)
            .HasConstraintName("FK_FlipRecordAddOn_FlipRecord");
    }
}
