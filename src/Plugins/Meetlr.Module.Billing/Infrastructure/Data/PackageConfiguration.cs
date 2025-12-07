using Meetlr.Module.Billing.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Meetlr.Module.Billing.Infrastructure.Data;

public class PackageConfiguration : IEntityTypeConfiguration<Package>
{
    public void Configure(EntityTypeBuilder<Package> builder)
    {
        builder.ToTable("Packages");

        builder.HasKey(p => p.Id);

        builder.Property(p => p.Name)
            .IsRequired()
            .HasMaxLength(100);

        builder.Property(p => p.Description)
            .IsRequired()
            .HasMaxLength(1000);

        builder.Property(p => p.Type)
            .IsRequired();

        builder.Property(p => p.Price)
            .IsRequired()
            .HasPrecision(18, 2);

        builder.Property(p => p.DurationDays)
            .IsRequired();

        builder.Property(p => p.IsActive)
            .IsRequired()
            .HasDefaultValue(true);

        builder.Property(p => p.SortOrder)
            .IsRequired()
            .HasDefaultValue(0);

        builder.Property(p => p.StripePriceId)
            .HasMaxLength(256);

        // INDEXES

        // Index for active packages sorted by order
        builder.HasIndex(p => new { p.IsActive, p.SortOrder })
            .HasDatabaseName("IX_Packages_Active_SortOrder");

        // Index for Stripe lookup
        builder.HasIndex(p => p.StripePriceId)
            .HasDatabaseName("IX_Packages_StripePriceId");
    }
}
