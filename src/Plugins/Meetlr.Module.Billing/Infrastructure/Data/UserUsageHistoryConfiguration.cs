using Meetlr.Module.Billing.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Meetlr.Module.Billing.Infrastructure.Data;

public class UserUsageHistoryConfiguration : IEntityTypeConfiguration<UserUsageHistory>
{
    public void Configure(EntityTypeBuilder<UserUsageHistory> builder)
    {
        builder.ToTable("UserUsageHistory");

        builder.HasKey(u => u.Id);

        builder.Property(u => u.UserId)
            .IsRequired();

        builder.Property(u => u.ServiceType)
            .IsRequired();

        builder.Property(u => u.CreditsUsed)
            .IsRequired();

        builder.Property(u => u.RelatedEntityType)
            .HasMaxLength(100);

        builder.Property(u => u.Recipient)
            .HasMaxLength(256);

        builder.Property(u => u.BalanceAfter)
            .IsRequired();

        builder.Property(u => u.UsedAt)
            .IsRequired();

        builder.Property(u => u.WasUnlimited)
            .IsRequired()
            .HasDefaultValue(false);

        // INDEXES

        // Index for user's usage history
        builder.HasIndex(u => u.UserId)
            .HasDatabaseName("IX_UserUsageHistory_UserId");

        // Index for filtering by service type
        builder.HasIndex(u => new { u.UserId, u.ServiceType })
            .HasDatabaseName("IX_UserUsageHistory_UserId_ServiceType");

        // Index for related entity lookup
        builder.HasIndex(u => new { u.RelatedEntityId, u.RelatedEntityType })
            .HasDatabaseName("IX_UserUsageHistory_RelatedEntity");

        // Index for date-based queries (reporting)
        builder.HasIndex(u => u.UsedAt)
            .HasDatabaseName("IX_UserUsageHistory_UsedAt");

        // Composite index for analytics (usage by service over time)
        builder.HasIndex(u => new { u.ServiceType, u.UsedAt })
            .HasDatabaseName("IX_UserUsageHistory_ServiceType_UsedAt");
    }
}
