using FlipLeo.Repository.Converters;
using FlipLeo.Repository.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace FlipLeo.Repository.Configurations;

public class UserAccountTokenConfiguration : IEntityTypeConfiguration<UserAccountToken>
{
    public void Configure(EntityTypeBuilder<UserAccountToken> builder)
    {
        builder.ToTable("UserAccountToken");

        builder.HasKey(e => e.Id);

        builder.HasIndex(e => e.TokenHash, "UQ_UserAccountToken_TokenHash").IsUnique();

        builder.Property(e => e.Purpose)
            .IsRequired()
            .HasMaxLength(50)
            .IsUnicode(false);

        builder.Property(e => e.TokenHash)
            .IsRequired()
            .HasMaxLength(64)
            .IsUnicode(false);

        builder.Property(e => e.ExpiresDate)
            .HasColumnType("datetime")
            .HasConversion<UtcDateTimeConverter>();

        builder.Property(e => e.UsedDate)
            .HasColumnType("datetime");

        builder.Property(e => e.CreatedDate)
            .HasColumnType("datetime")
            .HasConversion<UtcDateTimeConverter>();

        builder.HasOne(d => d.UserAccount).WithMany()
            .HasForeignKey(d => d.UserAccountId)
            .OnDelete(DeleteBehavior.ClientSetNull)
            .HasConstraintName("FK_UserAccountToken_UserAccount");
    }
}
