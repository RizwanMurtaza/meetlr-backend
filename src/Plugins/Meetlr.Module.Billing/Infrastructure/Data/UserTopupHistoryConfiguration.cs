using Meetlr.Module.Billing.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Meetlr.Module.Billing.Infrastructure.Data;

public class UserTopupHistoryConfiguration : IEntityTypeConfiguration<UserTopupHistory>
{
    public void Configure(EntityTypeBuilder<UserTopupHistory> builder)
    {
        builder.ToTable("UserTopupHistory");

        builder.HasKey(u => u.Id);

        builder.Property(u => u.UserId)
            .IsRequired();

        builder.Property(u => u.CreditsAdded)
            .IsRequired();

        builder.Property(u => u.AmountPaid)
            .IsRequired()
            .HasPrecision(18, 2);

        builder.Property(u => u.Currency)
            .IsRequired()
            .HasMaxLength(3)
            .HasDefaultValue("USD");

        builder.Property(u => u.TransactionType)
            .IsRequired();

        builder.Property(u => u.StripePaymentIntentId)
            .HasMaxLength(256);

        builder.Property(u => u.Description)
            .HasMaxLength(500);

        builder.Property(u => u.BalanceAfter)
            .IsRequired();

        // Relationships
        builder.HasOne(u => u.UserPackage)
            .WithMany()
            .HasForeignKey(u => u.UserPackageId)
            .OnDelete(DeleteBehavior.SetNull);

        // INDEXES

        // Index for user's transaction history
        builder.HasIndex(u => u.UserId)
            .HasDatabaseName("IX_UserTopupHistory_UserId");

        // Index for filtering by transaction type
        builder.HasIndex(u => new { u.UserId, u.TransactionType })
            .HasDatabaseName("IX_UserTopupHistory_UserId_TransactionType");

        // Index for Stripe payment lookup
        builder.HasIndex(u => u.StripePaymentIntentId)
            .HasDatabaseName("IX_UserTopupHistory_StripePaymentIntentId");

        // Index for date-based queries (reporting)
        builder.HasIndex(u => u.CreatedAt)
            .HasDatabaseName("IX_UserTopupHistory_CreatedAt");
    }
}
