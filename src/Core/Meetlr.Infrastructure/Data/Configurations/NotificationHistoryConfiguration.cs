 
using Meetlr.Domain.Entities.Notifications;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Meetlr.Infrastructure.Data.Configurations;

public class NotificationHistoryConfiguration : IEntityTypeConfiguration<NotificationHistory>
{
    public void Configure(EntityTypeBuilder<NotificationHistory> builder)
    {
        builder.ToTable("NotificationsHistory");

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

        builder.Property(n => n.FinalStatus)
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

        // Indexes for performance and reporting
        builder.HasIndex(n => n.FinalStatus);
        builder.HasIndex(n => n.ProcessedAt);
        builder.HasIndex(n => n.BookingId);
        builder.HasIndex(n => n.MeetlrEventId);
        builder.HasIndex(n => n.UserId);
        builder.HasIndex(n => new { n.UserId, n.ProcessedAt })
            .HasDatabaseName("IX_NotificationsHistory_UserReporting");
    }
}
