using Meetlr.Module.Billing.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Meetlr.Module.Billing.Infrastructure.Data;

public class UserCreditsConfiguration : IEntityTypeConfiguration<UserCredits>
{
    public void Configure(EntityTypeBuilder<UserCredits> builder)
    {
        builder.ToTable("UserCredits");

        builder.HasKey(u => u.Id);

        builder.Property(u => u.UserId)
            .IsRequired();

        builder.Property(u => u.Balance)
            .IsRequired()
            .HasDefaultValue(0);

        builder.Property(u => u.IsUnlimited)
            .IsRequired()
            .HasDefaultValue(false);

        builder.Property(u => u.Version)
            .IsRequired()
            .HasDefaultValue(1)
            .IsConcurrencyToken(); // Optimistic concurrency

        // INDEXES

        // Unique index on UserId - one credit record per user
        builder.HasIndex(u => u.UserId)
            .IsUnique()
            .HasDatabaseName("IX_UserCredits_UserId");

        // Index for finding unlimited users with expiration
        builder.HasIndex(u => new { u.IsUnlimited, u.UnlimitedExpiresAt })
            .HasDatabaseName("IX_UserCredits_Unlimited");
    }
}
