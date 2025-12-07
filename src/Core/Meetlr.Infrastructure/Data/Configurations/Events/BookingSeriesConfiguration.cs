using  Meetlr.Domain.Entities.Events;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Meetlr.Infrastructure.Data.Configurations.Events;

public class BookingSeriesConfiguration : IEntityTypeConfiguration<BookingSeries>
{
    public void Configure(EntityTypeBuilder<BookingSeries> builder)
    {
        builder.ToTable("BookingSeries");

        builder.HasKey(s => s.Id);

        // Invitee fields removed - all invitee info comes from Contact entity

        builder.Property(s => s.OccurrenceCount)
            .IsRequired();

        builder.Property(s => s.TotalOccurrences)
            .IsRequired();

        builder.Property(s => s.PaymentType)
            .IsRequired();

        builder.Property(s => s.SubscriptionId)
            .HasMaxLength(255);

        builder.Property(s => s.PaymentStatus)
            .IsRequired();

        builder.Property(s => s.PaymentProviderType)
            .HasConversion<int?>();

        builder.Property(s => s.TotalAmount)
            .HasPrecision(18, 2);

        builder.Property(s => s.Currency)
            .HasMaxLength(3);

        builder.Property(s => s.PaymentIntentId)
            .HasMaxLength(255);

        builder.Property(s => s.Status)
            .IsRequired();

        // ============================================
        // OPTIMIZED INDEX STRATEGY - 7 Strategic Indexes
        // ============================================

        // 1. PRIMARY TENANT-HOST INDEX - Covers: tenant series by host with status filtering
        // Supports: Host series dashboard, series list, status filtering
        builder.HasIndex(s => new { s.TenantId, s.HostUserId, s.Status })
            .HasDatabaseName("IX_BookingSeries_TenantId_Host_Status");

        // 2. HOST SERIES INDEX - Covers: host's series with creation date sorting
        // Supports: Host series management, newest/oldest sorting
        builder.HasIndex(s => new { s.HostUserId, s.Status, s.CreatedAt })
            .HasDatabaseName("IX_BookingSeries_HostUserId_Status_CreatedAt");

        // 3. PAYMENT STATUS INDEX - Covers: payment tracking and filtering
        // Supports: Payment status reports, subscription management
        builder.HasIndex(s => new { s.TenantId, s.PaymentStatus, s.PaymentType })
            .HasDatabaseName("IX_BookingSeries_TenantId_PaymentStatus_Type");

        // 4. SUBSCRIPTION LOOKUP INDEX - Stripe/PayPal subscription lookups
        // Supports: Webhook processing, subscription queries
        builder.HasIndex(s => s.SubscriptionId)
            .HasDatabaseName("IX_BookingSeries_SubscriptionId")
            .HasFilter("[SubscriptionId] IS NOT NULL");

        // 5. PAYMENT INTENT INDEX - Payment intent lookups
        // Supports: Payment webhook handlers
        builder.HasIndex(s => s.PaymentIntentId)
            .HasDatabaseName("IX_BookingSeries_PaymentIntentId")
            .HasFilter("[PaymentIntentId] IS NOT NULL");

        // 6. EVENT TYPE INDEX - Series by event type
        // Supports: Event analytics, series grouping by event
        builder.HasIndex(s => new { s.BaseMeetlrEventId, s.Status })
            .HasDatabaseName("IX_BookingSeries_EventId_Status");

        // 7. CONTACT INDEX - Find series by contact
        // Supports: Contact booking history
        builder.HasIndex(s => new { s.TenantId, s.ContactId })
            .HasDatabaseName("IX_BookingSeries_TenantId_ContactId");

        // Relationships
        builder.HasOne(s => s.Contact)
            .WithMany()
            .HasForeignKey(s => s.ContactId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(s => s.BaseMeetlrEvent)
            .WithMany()
            .HasForeignKey(s => s.BaseMeetlrEventId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(s => s.HostUser)
            .WithMany()
            .HasForeignKey(s => s.HostUserId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasMany(s => s.Bookings)
            .WithOne(b => b.BookingSeries)
            .HasForeignKey(b => b.SeriesBookingId)
            .OnDelete(DeleteBehavior.SetNull);
    }
}
