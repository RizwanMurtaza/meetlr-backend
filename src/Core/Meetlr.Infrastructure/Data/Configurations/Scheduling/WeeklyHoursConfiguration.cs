using  Meetlr.Domain.Entities.Scheduling;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Meetlr.Infrastructure.Data.Configurations.Scheduling;

public class WeeklyHoursConfiguration : IEntityTypeConfiguration<WeeklyHours>
{
    public void Configure(EntityTypeBuilder<WeeklyHours> builder)
    {
        builder.ToTable("WeeklyHours");

        builder.HasKey(w => w.Id);

        builder.Property(w => w.DayOfWeek)
            .IsRequired();

        builder.Property(w => w.IsAvailable)
            .IsRequired();

        builder.Property(w => w.StartTime)
            .IsRequired();

        builder.Property(w => w.EndTime)
            .IsRequired();

        // ============================================
        // OPTIMIZED INDEX STRATEGY - 2 Strategic Indexes
        // ============================================

        // 1. PRIMARY SCHEDULE INDEX - Covers: schedule's weekly hours with day and availability
        // Supports: Weekly hours lookup by schedule, day-specific queries, availability filtering
        builder.HasIndex(w => new { w.AvailabilityScheduleId, w.DayOfWeek, w.IsAvailable })
            .HasDatabaseName("IX_WeeklyHours_ScheduleId_DayOfWeek_IsAvailable");

        // 2. AVAILABLE HOURS INDEX - Quick lookup of available time slots
        // Supports: Availability calculation, slot finding
        builder.HasIndex(w => new { w.AvailabilityScheduleId, w.IsAvailable, w.StartTime })
            .HasDatabaseName("IX_WeeklyHours_ScheduleId_IsAvailable_StartTime")
            .HasFilter("[IsAvailable] = 1");

        // Relationships
        builder.HasOne(w => w.AvailabilitySchedule)
            .WithMany(a => a.WeeklyHours)
            .HasForeignKey(w => w.AvailabilityScheduleId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
