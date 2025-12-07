using  Meetlr.Domain.Entities.Events;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Meetlr.Infrastructure.Data.Configurations.Events;

public class MeetlrEventConfiguration : IEntityTypeConfiguration<MeetlrEvent>
{
    public void Configure(EntityTypeBuilder<MeetlrEvent> builder)
    {
        builder.ToTable("MeetlrEvents");

        builder.HasKey(e => e.Id);

        builder.Property(e => e.Name)
            .IsRequired()
            .HasMaxLength(200);

        builder.Property(e => e.Description)
            .HasMaxLength(2000);

        builder.Property(e => e.MeetingLocationType)
            .IsRequired();

        builder.Property(e => e.LocationDetails)
            .HasMaxLength(500);

        builder.Property(e => e.Slug)
            .IsRequired()
            .HasMaxLength(200);

        builder.Property(e => e.Color)
            .HasMaxLength(20);

        builder.Property(e => e.Status)
            .IsRequired();

        builder.Property(e => e.IsActive)
            .IsRequired();

        builder.Property(e => e.DurationMinutes)
            .IsRequired();

        builder.Property(e => e.Fee)
            .HasPrecision(18, 2);

        builder.Property(e => e.Currency)
            .HasMaxLength(3);

        builder.Property(e => e.AvailabilityScheduleId)
            .IsRequired();

        builder.Property(e => e.PaymentProviderType)
            .HasConversion<int?>();

        // ============================================
        // OPTIMIZED INDEX STRATEGY - 7 Strategic Indexes
        // ============================================

        // 1. PRIMARY TENANT INDEX - Covers: tenant-wide event queries with filtering
        // Supports: Dashboard, tenant event lists, active/status filtering
        builder.HasIndex(e => new { e.TenantId, e.IsActive, e.Status })
            .HasDatabaseName("IX_MeetlrEvents_TenantId_IsActive_Status");

        // 2. OWNER EVENTS INDEX - Covers: user's event list with filtering
        // Supports: User event dashboard, active events filtering
        builder.HasIndex(e => new { e.UserId, e.IsActive, e.Status })
            .HasDatabaseName("IX_MeetlrEvents_UserId_IsActive_Status");

        // 3. UNIQUE SLUG INDEX - UNIQUE constraint for user + slug
        // Supports: Public booking URLs (username/slug), slug uniqueness validation
        builder.HasIndex(e => new { e.UserId, e.Slug })
            .IsUnique()
            .HasDatabaseName("IX_MeetlrEvents_UserId_Slug");

        // 4. SCHEDULE RELATIONSHIP INDEX - Foreign key index
        // Supports: Availability schedule lookups, cascade queries
        builder.HasIndex(e => e.AvailabilityScheduleId)
            .HasDatabaseName("IX_MeetlrEvents_AvailabilityScheduleId");

        // 5. PAID EVENTS INDEX - Filter events by payment requirement
        // Supports: Payment analytics, paid event filtering
        builder.HasIndex(e => new { e.UserId, e.RequiresPayment, e.IsActive })
            .HasDatabaseName("IX_MeetlrEvents_UserId_Payment_IsActive")
            .HasFilter("[RequiresPayment] = 1");

        // 6. SLUG LOOKUP INDEX - Fast slug searches for public booking pages
        // Supports: Public booking page routing
        builder.HasIndex(e => e.Slug)
            .HasDatabaseName("IX_MeetlrEvents_Slug");

        // 7. EVENT ANALYTICS INDEX - Creation date sorting and filtering
        // Supports: Event creation analytics, newest/oldest sorting
        builder.HasIndex(e => new { e.TenantId, e.CreatedAt })
            .HasDatabaseName("IX_MeetlrEvents_TenantId_CreatedAt");

        // Relationships
        builder.HasOne(e => e.User)
            .WithMany(u => u.MeetlrEvents)
            .HasForeignKey(e => e.UserId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(e => e.AvailabilitySchedule)
            .WithMany(a => a.MeetlrEvents)
            .HasForeignKey(e => e.AvailabilityScheduleId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasMany(e => e.Questions)
            .WithOne(q => q.MeetlrEvent)
            .HasForeignKey(q => q.MeetlrEventId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasMany(e => e.Bookings)
            .WithOne(b => b.MeetlrEvent)
            .HasForeignKey(b => b.MeetlrEventId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
