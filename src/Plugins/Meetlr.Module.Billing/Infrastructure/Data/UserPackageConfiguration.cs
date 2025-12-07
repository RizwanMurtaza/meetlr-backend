using Meetlr.Module.Billing.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Meetlr.Module.Billing.Infrastructure.Data;

public class UserPackageConfiguration : IEntityTypeConfiguration<UserPackage>
{
    public void Configure(EntityTypeBuilder<UserPackage> builder)
    {
        builder.ToTable("UserPackages");

        builder.HasKey(u => u.Id);

        builder.Property(u => u.UserId)
            .IsRequired();

        builder.Property(u => u.PackageId)
            .IsRequired();

        builder.Property(u => u.Status)
            .IsRequired();

        builder.Property(u => u.StartDate)
            .IsRequired();

        builder.Property(u => u.EndDate)
            .IsRequired();

        builder.Property(u => u.CreditsGranted)
            .IsRequired()
            .HasDefaultValue(0);

        builder.Property(u => u.CreditsUsed)
            .IsRequired()
            .HasDefaultValue(0);

        builder.Property(u => u.StripeSubscriptionId)
            .HasMaxLength(256);

        builder.Property(u => u.CancellationReason)
            .HasMaxLength(500);

        // Relationships
        builder.HasOne(u => u.Package)
            .WithMany()
            .HasForeignKey(u => u.PackageId)
            .OnDelete(DeleteBehavior.Restrict);

        // INDEXES

        // Index for user's active subscription
        builder.HasIndex(u => new { u.UserId, u.Status })
            .HasDatabaseName("IX_UserPackages_UserId_Status");

        // Index for expiring subscriptions (background job)
        builder.HasIndex(u => new { u.Status, u.EndDate })
            .HasDatabaseName("IX_UserPackages_Status_EndDate");

        // Index for Stripe subscription lookup
        builder.HasIndex(u => u.StripeSubscriptionId)
            .HasDatabaseName("IX_UserPackages_StripeSubscriptionId");
    }
}
