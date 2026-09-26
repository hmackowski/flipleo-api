using FlipLeo.Repository.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace FlipLeo.Repository.Configurations;

public class LookupAuctionSiteConfiguration : IEntityTypeConfiguration<LookupAuctionSite>
{
    public void Configure(EntityTypeBuilder<LookupAuctionSite> builder)
    {
        builder.ToTable("LookupAuctionSite");

        builder.HasKey(e => e.Id);

        builder.HasIndex(e => e.Name, "UQ_LookupAuctionSite_Name").IsUnique();

        builder.Property(e => e.Name)
            .IsRequired()
            .HasMaxLength(50)
            .IsUnicode(false);

        builder.Property(e => e.WebsiteUrl)
            .HasMaxLength(1000)
            .IsUnicode(false);
    }
}
