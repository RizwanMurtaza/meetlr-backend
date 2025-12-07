 
using Meetlr.Domain.Entities.Notifications;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Meetlr.Infrastructure.Data.Configurations;

public class NotificationPendingConfiguration : IEntityTypeConfiguration<NotificationPending>
{
    public void Configure(EntityTypeBuilder<NotificationPending> builder)
    {
        builder.ToTable("NotificationsPending");

        builder.HasKey(n => n.Id);

        // BookingId is nullable - slot invitations don't have a booking yet
        builder.Property(n => n.BookingId)
            .IsRequired(false);

        builder.Property(n => n.MeetlrEventId)
            .IsRequired();

        builder.Property(n => n.UserId)
            .IsRequired();

        builder.Property(n => n.NotificationType)
            .IsRequired()
            .HasConversion<int>();

        builder.Property(n => n.Trigger)
            .IsRequired()
            .HasConversion<int>();

        builder.Property(n => n.Status)
            .IsRequired()
            .HasConversion<int>();

        builder.Property(n => n.Recipient)
            .IsRequired()
            .HasMaxLength(500);

        builder.Property(n => n.PayloadJson)
            .IsRequired()
            .HasColumnType("text");

        builder.Property(n => n.ErrorMessage)
            .HasMaxLength(2000);

        builder.Property(n => n.ErrorDetails)
            .HasColumnType("text");

        builder.Property(n => n.ExternalMessageId)
            .HasMaxLength(255);

        // ============================================
        // OPTIMIZED INDEX STRATEGY - 5 Strategic Indexes
        // ============================================

        // 1. NOTIFICATION PROCESSING INDEX - Critical for background job processing
        // Supports: Notification queue processing, retry logic, execution scheduling
        builder.HasIndex(n => new { n.Status, n.ExecuteAt, n.NextRetryAt })
            .HasDatabaseName("IX_NotificationsPending_Processing");

        // 2. TENANT NOTIFICATIONS INDEX - Tenant-wide notification queries
        // Supports: Tenant notification dashboard, status filtering, execution scheduling
        builder.HasIndex(n => new { n.TenantId, n.Status, n.ExecuteAt })
            .HasDatabaseName("IX_NotificationsPending_TenantId_Status_ExecuteAt");

        // 3. BOOKING NOTIFICATIONS INDEX - All notifications for a specific booking
        // Supports: Booking notification history, notification cancellation
        builder.HasIndex(n => new { n.BookingId, n.Status })
            .HasDatabaseName("IX_NotificationsPending_BookingId_Status");

        // 4. USER NOTIFICATIONS INDEX - User-specific notification queries
        // Supports: User notification preferences, notification history
        builder.HasIndex(n => new { n.UserId, n.Status, n.ExecuteAt })
            .HasDatabaseName("IX_NotificationsPending_UserId_Status_ExecuteAt");

        // 5. EVENT NOTIFICATIONS INDEX - Notifications by event type
        // Supports: Event-level notification analytics
        builder.HasIndex(n => new { n.MeetlrEventId, n.Status })
            .HasDatabaseName("IX_NotificationsPending_EventId_Status");
    }
}
