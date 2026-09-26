using FlipLeo.Repository.Converters;
using FlipLeo.Repository.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace FlipLeo.Repository.Configurations;

public class UserAccountConfiguration : IEntityTypeConfiguration<UserAccount>
{
    public void Configure(EntityTypeBuilder<UserAccount> builder)
    {
        builder.ToTable("UserAccount");

        builder.HasKey(e => e.Id);

        builder.HasIndex(e => e.Email, "UQ_UserAccount_Email").IsUnique();

        builder.Property(e => e.Email)
            .IsRequired()
            .HasMaxLength(255)
            .IsUnicode(false);

        builder.Property(e => e.DisplayName)
            .IsRequired()
            .HasMaxLength(100)
            .IsUnicode(false);

        builder.Property(e => e.PasswordHash)
            .IsRequired()
            .HasMaxLength(500)
            .IsUnicode(false);

        builder.Property(e => e.LastLoginDate)
            .HasColumnType("datetime");

        builder.Property(e => e.CreatedDate)
            .HasColumnType("datetime")
            .HasConversion<UtcDateTimeConverter>();

        builder.Property(e => e.UpdatedDate)
            .HasColumnType("datetime")
            .HasConversion<UtcDateTimeConverter>();
    }
}
