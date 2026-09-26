using FlipLeo.Repository.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace FlipLeo.Repository.Configurations;

public class AddOnPresetConfiguration : IEntityTypeConfiguration<AddOnPreset>
{
    public void Configure(EntityTypeBuilder<AddOnPreset> builder)
    {
        builder.ToTable("AddOnPreset");

        builder.HasKey(e => e.Id);

        builder.HasQueryFilter(e => e.IsActive);

        builder.Property(e => e.Name)
            .IsRequired()
            .HasMaxLength(200)
            .IsUnicode(false);

        builder.Property(e => e.DefaultPrice)
            .HasColumnType("decimal(10, 2)");

        builder.Property(e => e.Link)
            .HasMaxLength(1000)
            .IsUnicode(false);

        builder.Property(e => e.ImageUrl)
            .HasMaxLength(1000)
            .IsUnicode(false);

        builder.ConfigureAuditColumns();

        builder.HasOne<UserAccount>().WithMany()
            .HasForeignKey(d => d.UserId)
            .OnDelete(DeleteBehavior.ClientSetNull)
            .HasConstraintName("FK_AddOnPreset_UserAccount");
    }
}
