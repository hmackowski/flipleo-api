using FlipLeo.Repository.Converters;
using FlipLeo.Repository.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace FlipLeo.Repository.Configurations;

/// <summary>
/// The audit columns are identical on every table, so they're configured once here
/// instead of being repeated in each configuration class.
/// </summary>
public static class AuditColumnsExtensions
{
    public static void ConfigureAuditColumns<TEntity>(this EntityTypeBuilder<TEntity> builder)
        where TEntity : EntityBase
    {
        builder.Property(e => e.CreatedByUsername)
            .IsRequired()
            .HasMaxLength(255)
            .IsUnicode(false);

        builder.Property(e => e.CreatedDate)
            .HasDefaultValueSql("(getdate())")
            .HasColumnType("datetime")
            .HasConversion<UtcDateTimeConverter>();

        builder.Property(e => e.UpdatedByUsername)
            .IsRequired()
            .HasMaxLength(255)
            .IsUnicode(false);

        builder.Property(e => e.UpdatedDate)
            .HasDefaultValueSql("(getdate())")
            .HasColumnType("datetime")
            .HasConversion<UtcDateTimeConverter>();
    }
}
