using  Meetlr.Domain.Entities.Events;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Meetlr.Infrastructure.Data.Configurations.Events;

public class BookingConfiguration : IEntityTypeConfiguration<Booking>
{
    public void Configure(EntityTypeBuilder<Booking> builder)
    {
        builder.ToTable("Bookings");

        builder.HasKey(b => b.Id);

        // Invitee fields removed - all invitee info comes from Contact entity

        builder.Property(b => b.StartTime)
            .IsRequired();

        builder.Property(b => b.EndTime)
            .IsRequired();

        builder.Property(b => b.Status)
            .IsRequired();

        builder.Property(b => b.Location)
            .HasMaxLength(500);

        builder.Property(b => b.MeetingLink)
            .HasMaxLength(1000);

        builder.Property(b => b.Notes)
            .HasMaxLength(2000);

        builder.Property(b => b.CancellationReason)
            .HasMaxLength(1000);

        builder.Property(b => b.ConfirmationToken)
            .IsRequired()
            .HasMaxLength(100);

        builder.Property(b => b.CancellationToken)
            .IsRequired()
            .HasMaxLength(100);

        builder.Property(b => b.PaymentStatus)
            .IsRequired();

        builder.Property(b => b.Amount)
            .HasPrecision(18, 2);

        builder.Property(b => b.Currency)
            .HasMaxLength(3);

        builder.Property(b => b.PaymentIntentId)
            .HasMaxLength(255);

        builder.Property(b => b.SeriesPaymentIntentId)
            .HasMaxLength(255);

        builder.Property(b => b.AllocatedAmount)
            .HasPrecision(18, 2);

        builder.Property(b => b.CalendarEventId)
            .HasMaxLength(255);

        // Rescheduling fields
        builder.Property(b => b.CancelledBy)
            .HasMaxLength(50);

        // No-show fields
        builder.Property(b => b.NoShowNotes)
            .HasMaxLength(1000);

        // Check-in fields
        builder.Property(b => b.CheckInMethod)
            .HasMaxLength(50);

        // Recording fields
        builder.Property(b => b.RecordingUrl)
            .HasMaxLength(1000);

        builder.Property(b => b.TranscriptUrl)
            .HasMaxLength(1000);

        builder.Property(b => b.RecordingStatus)
            .HasMaxLength(50);

        // Source tracking fields
        builder.Property(b => b.BookingSource)
            .HasMaxLength(100);

        builder.Property(b => b.ReferrerUrl)
            .HasMaxLength(2000);

        builder.Property(b => b.UtmSource)
            .HasMaxLength(255);

        builder.Property(b => b.UtmMedium)
            .HasMaxLength(255);

        builder.Property(b => b.UtmCampaign)
            .HasMaxLength(255);

        // Additional payment details
        builder.Property(b => b.DiscountAmount)
            .HasPrecision(18, 2);

        builder.Property(b => b.DiscountCode)
            .HasMaxLength(100);

        builder.Property(b => b.TaxAmount)
            .HasPrecision(18, 2);

        // Internal notes and custom data
        builder.Property(b => b.InternalNotes)
            .HasMaxLength(2000);

        builder.Property(b => b.CustomFieldsJson)
            .HasColumnType("text");

        // ============================================
        // OPTIMIZED INDEX STRATEGY - 10 Strategic Indexes
        // ============================================
        // Principle: Multi-column indexes cover most common query patterns
        // Each index is designed to support multiple query types

        // 1. PRIMARY SEARCH INDEX - Covers: tenant filtering, status filtering, time-based queries, sorting
        // Supports: Dashboard, list views, calendar views, conflict detection
        builder.HasIndex(b => new { b.TenantId, b.Status, b.StartTime })
            .HasDatabaseName("IX_Bookings_TenantId_Status_StartTime");

        // 2. HOST VIEW INDEX - Covers: host's bookings, time range queries
        // Supports: Host dashboard, host calendar, availability checking
        builder.HasIndex(b => new { b.TenantId, b.HostUserId, b.StartTime })
            .HasDatabaseName("IX_Bookings_TenantId_Host_StartTime");

        // 3. EVENT TYPE INDEX - Covers: bookings by event type, status filtering
        // Supports: Event analytics, event management
        builder.HasIndex(b => new { b.MeetlrEventId, b.Status, b.StartTime })
            .HasDatabaseName("IX_Bookings_MeetlrEvent_Status_StartTime");

        // 4. CONTACT INDEX - Covers: bookings by contact (customer history)
        // Supports: Contact profile, booking history per contact
        builder.HasIndex(b => new { b.TenantId, b.ContactId, b.StartTime })
            .HasDatabaseName("IX_Bookings_TenantId_ContactId_StartTime")
            .HasFilter("[ContactId] IS NOT NULL");

        // 5. SERIES INDEX - Covers: recurring booking management
        // Supports: Series management, recurring booking queries
        builder.HasIndex(b => new { b.SeriesBookingId, b.OccurrenceIndex })
            .HasDatabaseName("IX_Bookings_SeriesId_OccurrenceIndex")
            .HasFilter("[SeriesBookingId] IS NOT NULL");

        // 6. CONFIRMATION TOKEN INDEX - UNIQUE constraint for booking confirmation
        // Supports: Public booking confirmation pages
        builder.HasIndex(b => b.ConfirmationToken)
            .IsUnique()
            .HasDatabaseName("IX_Bookings_ConfirmationToken");

        // 7. CANCELLATION TOKEN INDEX - UNIQUE constraint for booking cancellation
        // Supports: Public booking cancellation pages
        builder.HasIndex(b => b.CancellationToken)
            .IsUnique()
            .HasDatabaseName("IX_Bookings_CancellationToken");

        // 8. PAYMENT LOOKUP INDEX - Covers: payment intent lookups (Stripe webhooks)
        // Supports: Payment processing, webhook handlers
        builder.HasIndex(b => b.PaymentIntentId)
            .HasDatabaseName("IX_Bookings_PaymentIntentId")
            .HasFilter("[PaymentIntentId] IS NOT NULL");

        // 9. NO-SHOW TRACKING INDEX - Covers: no-show reports and analytics
        // Supports: Contact reliability scoring, no-show reports
        builder.HasIndex(b => new { b.TenantId, b.IsNoShow, b.ContactId })
            .HasDatabaseName("IX_Bookings_TenantId_NoShow_ContactId")
            .HasFilter("[IsNoShow] = 1");

        // 10. RESCHEDULING CHAIN INDEX - Covers: booking reschedule history
        // Supports: Rescheduling workflows, booking history tracking
        builder.HasIndex(b => b.RescheduledFromBookingId)
            .HasDatabaseName("IX_Bookings_RescheduledFromBookingId")
            .HasFilter("[RescheduledFromBookingId] IS NOT NULL");

        builder.Property(b => b.PaymentProviderType)
            .HasConversion<int?>();

        // Relationships
        builder.HasOne(b => b.Contact)
            .WithMany(c => c.Bookings)
            .HasForeignKey(b => b.ContactId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(b => b.MeetlrEvent)
            .WithMany(e => e.Bookings)
            .HasForeignKey(b => b.MeetlrEventId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(b => b.HostUser)
            .WithMany(u => u.Bookings)
            .HasForeignKey(b => b.HostUserId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(b => b.BookingSeries)
            .WithMany(s => s.Bookings)
            .HasForeignKey(b => b.SeriesBookingId)
            .OnDelete(DeleteBehavior.SetNull);

        builder.HasMany(b => b.Answers)
            .WithOne(a => a.Booking)
            .HasForeignKey(a => a.BookingId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
