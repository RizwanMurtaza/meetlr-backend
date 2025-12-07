using  Meetlr.Domain.Entities.Scheduling;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Meetlr.Infrastructure.Data.Configurations.Scheduling;

public class DateSpecificHoursConfiguration : IEntityTypeConfiguration<DateSpecificHours>
{
    public void Configure(EntityTypeBuilder<DateSpecificHours> builder)
    {
        builder.ToTable("DateSpecificHours");

        builder.HasKey(d => d.Id);

        builder.Property(d => d.Date)
            .IsRequired();

        builder.Property(d => d.IsAvailable)
            .IsRequired();

        builder.Property(d => d.StartTime)
            .IsRequired(false);

        builder.Property(d => d.EndTime)
            .IsRequired(false);

        // ============================================
        // OPTIMIZED INDEX STRATEGY - 2 Strategic Indexes
        // ============================================

        // 1. PRIMARY SCHEDULE-DATE INDEX - Covers: date overrides by schedule
        // Supports: Date-specific availability lookups, override queries, availability filtering
        builder.HasIndex(d => new { d.AvailabilityScheduleId, d.Date, d.IsAvailable })
            .HasDatabaseName("IX_DateSpecificHours_ScheduleId_Date_IsAvailable");

        // 2. DATE RANGE INDEX - Date-based queries across schedules
        // Supports: Finding all overrides for a date range, calendar view queries
        builder.HasIndex(d => new { d.Date, d.IsAvailable })
            .HasDatabaseName("IX_DateSpecificHours_Date_IsAvailable");

        // Relationships
        builder.HasOne(d => d.AvailabilitySchedule)
            .WithMany(a => a.DateSpecificHours)
            .HasForeignKey(d => d.AvailabilityScheduleId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
