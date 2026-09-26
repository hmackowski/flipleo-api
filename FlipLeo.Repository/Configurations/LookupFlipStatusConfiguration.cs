using FlipLeo.Repository.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace FlipLeo.Repository.Configurations;

public class LookupFlipStatusConfiguration : IEntityTypeConfiguration<LookupFlipStatus>
{
    public void Configure(EntityTypeBuilder<LookupFlipStatus> builder)
    {
        builder.ToTable("LookupFlipStatus");

        builder.HasKey(e => e.Id);

        builder.HasIndex(e => e.Name, "UQ_LookupFlipStatus_Name").IsUnique();

        builder.Property(e => e.Name)
            .IsRequired()
            .HasMaxLength(50)
            .IsUnicode(false);
    }
}
