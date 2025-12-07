using Meetlr.Module.Calendar.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Meetlr.Module.Calendar.Infrastructure.Configurations;

public class CalendarIntegrationConfiguration : IEntityTypeConfiguration<CalendarIntegration>
{
    public void Configure(EntityTypeBuilder<CalendarIntegration> builder)
    {
        builder.ToTable("CalendarIntegrations");

        builder.HasKey(c => c.Id);

        builder.Property(c => c.Provider)
            .IsRequired();

        builder.Property(c => c.ProviderEmail)
            .IsRequired()
            .HasMaxLength(255);

        builder.Property(c => c.ProviderCalendarId)
            .IsRequired()
            .HasMaxLength(255);

        builder.Property(c => c.AccessToken)
            .IsRequired()
            .HasMaxLength(2000);

        builder.Property(c => c.RefreshToken)
            .HasMaxLength(2000);

        builder.Property(c => c.IsActive)
            .IsRequired();

        builder.Property(c => c.CheckForConflicts)
            .IsRequired();

        builder.Property(c => c.AddEventsToCalendar)
            .IsRequired();

        // OPTIMIZED INDEX STRATEGY - 3 Strategic Indexes

        // 1. PRIMARY SCHEDULE INDEX - Covers: schedule's calendar integrations with active/provider filtering
        builder.HasIndex(c => new { c.AvailabilityScheduleId, c.IsActive, c.Provider })
            .HasDatabaseName("IX_CalendarIntegrations_ScheduleId_IsActive_Provider");

        // 2. TOKEN REFRESH INDEX - Find integrations needing token refresh
        builder.HasIndex(c => new { c.TokenExpiresAt, c.IsActive })
            .HasDatabaseName("IX_CalendarIntegrations_TokenExpiresAt_IsActive")
            .HasFilter("[IsActive] = 1");

        // 3. PROVIDER EMAIL INDEX - Lookup by provider email
        builder.HasIndex(c => new { c.ProviderEmail, c.Provider })
            .HasDatabaseName("IX_CalendarIntegrations_ProviderEmail_Provider");
    }
}
