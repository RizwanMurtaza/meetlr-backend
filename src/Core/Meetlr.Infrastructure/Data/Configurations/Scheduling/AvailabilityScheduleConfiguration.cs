using  Meetlr.Domain.Entities.Scheduling;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Meetlr.Infrastructure.Data.Configurations.Scheduling;

public class AvailabilityScheduleConfiguration : IEntityTypeConfiguration<AvailabilitySchedule>
{
    public void Configure(EntityTypeBuilder<AvailabilitySchedule> builder)
    {
        builder.ToTable("AvailabilitySchedules");

        builder.HasKey(a => a.Id);

        builder.Property(a => a.Name)
            .IsRequired()
            .HasMaxLength(200);

        builder.Property(a => a.TimeZone)
            .IsRequired()
            .HasMaxLength(100);

        builder.Property(a => a.IsDefault)
            .IsRequired();

        // ============================================
        // OPTIMIZED INDEX STRATEGY - 3 Strategic Indexes
        // ============================================

        // 1. PRIMARY TENANT-USER INDEX - Covers: tenant schedules by user
        // Supports: User schedule list, tenant-wide schedule queries
        builder.HasIndex(a => new { a.TenantId, a.UserId, a.IsDefault })
            .HasDatabaseName("IX_AvailabilitySchedules_TenantId_UserId_IsDefault");

        // 2. USER SCHEDULES INDEX - Covers: user's schedules with sorting
        // Supports: User schedule management, default schedule lookup, creation date sorting
        builder.HasIndex(a => new { a.UserId, a.IsDefault, a.CreatedAt })
            .HasDatabaseName("IX_AvailabilitySchedules_UserId_IsDefault_CreatedAt");

        // 3. SCHEDULE TYPE INDEX - Filter schedules by type
        // Supports: Schedule type filtering (Personal, Team, etc.)
        builder.HasIndex(a => new { a.TenantId, a.ScheduleType })
            .HasDatabaseName("IX_AvailabilitySchedules_TenantId_ScheduleType");

        // Relationships
        builder.HasOne(a => a.User)
            .WithMany(u => u.AvailabilitySchedules)
            .HasForeignKey(a => a.UserId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasMany(a => a.WeeklyHours)
            .WithOne(w => w.AvailabilitySchedule)
            .HasForeignKey(w => w.AvailabilityScheduleId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasMany(a => a.DateSpecificHours)
            .WithOne(d => d.AvailabilitySchedule)
            .HasForeignKey(d => d.AvailabilityScheduleId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasMany(a => a.MeetlrEvents)
            .WithOne(e => e.AvailabilitySchedule)
            .HasForeignKey(e => e.AvailabilityScheduleId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
