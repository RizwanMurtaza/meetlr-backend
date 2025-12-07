using  Meetlr.Domain.Entities.Users;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Meetlr.Infrastructure.Data.Configurations.Users;

public class UserSettingsConfiguration : IEntityTypeConfiguration<UserSettings>
{
    public void Configure(EntityTypeBuilder<UserSettings> builder)
    {
        builder.ToTable("UserSettings");

        builder.HasKey(us => us.Id);

        // One-to-One relationship with User
        builder.HasOne(us => us.User)
            .WithOne(u => u.UserSettings)
            .HasForeignKey<UserSettings>(us => us.UserId)
            .OnDelete(DeleteBehavior.Cascade);

        // Currency & Payments
        builder.Property(us => us.DefaultCurrency)
            .IsRequired()
            .HasMaxLength(3) // ISO 4217
            .HasDefaultValue("USD");

        // Notification Defaults
        builder.Property(us => us.DefaultNotifyViaEmail)
            .IsRequired()
            .HasDefaultValue(true);

        builder.Property(us => us.DefaultNotifyViaSms)
            .IsRequired()
            .HasDefaultValue(false);

        builder.Property(us => us.DefaultNotifyViaWhatsApp)
            .IsRequired()
            .HasDefaultValue(false);

        // Notification Master Switches
        builder.Property(us => us.EmailNotificationsEnabled)
            .IsRequired()
            .HasDefaultValue(true);

        builder.Property(us => us.SmsNotificationsEnabled)
            .IsRequired()
            .HasDefaultValue(false);

        builder.Property(us => us.WhatsAppNotificationsEnabled)
            .IsRequired()
            .HasDefaultValue(false);

        // Meeting Defaults
        builder.Property(us => us.DefaultEventDuration)
            .IsRequired()
            .HasDefaultValue(30);

        builder.Property(us => us.DefaultBufferBefore)
            .IsRequired()
            .HasDefaultValue(0);

        builder.Property(us => us.DefaultBufferAfter)
            .IsRequired()
            .HasDefaultValue(0);

        builder.Property(us => us.DefaultMinBookingNotice)
            .IsRequired()
            .HasDefaultValue(60);

        builder.Property(us => us.DefaultReminderHours)
            .IsRequired()
            .HasDefaultValue(24);

        // Location Defaults
        // Note: HasSentinel(0) tells EF that when value is 0 (CLR default, not a valid enum value),
        // it should use the database default. This prevents the warning about sentinel values.
        builder.Property(us => us.DefaultMeetingLocationType)
            .IsRequired()
            .HasDefaultValue(Domain.Enums.MeetingLocationType.Zoom)
            .HasSentinel((Domain.Enums.MeetingLocationType)0)
            .HasConversion<int>();

        builder.Property(us => us.DefaultLocationDetails)
            .HasMaxLength(500);

        // Calendar Preferences
        builder.Property(us => us.WeekStartsOn)
            .IsRequired()
            .HasDefaultValue(Domain.Enums.WeekStartsOn.Sunday)
            .HasConversion<int>();

        // UX Settings
        builder.Property(us => us.ProfileCompleted)
            .IsRequired()
            .HasDefaultValue(false);

        builder.Property(us => us.OnboardingStep)
            .IsRequired()
            .HasDefaultValue(0);

        builder.Property(us => us.BookingPageTheme)
            .HasMaxLength(50)
            .HasDefaultValue("default");

        builder.Property(us => us.ShowPoweredBy)
            .IsRequired()
            .HasDefaultValue(true);

        // Professional Info
        builder.Property(us => us.JobTitle)
            .HasMaxLength(100);

        builder.Property(us => us.WebsiteUrl)
            .HasMaxLength(500);

        builder.Property(us => us.LinkedInUrl)
            .HasMaxLength(500);

        builder.Property(us => us.TwitterUrl)
            .HasMaxLength(500);

        // Indexes
        // Primary tenant index
        builder.HasIndex(us => us.TenantId)
            .HasDatabaseName("IX_UserSettings_TenantId");

        builder.HasIndex(us => new { us.TenantId, us.UserId })
            .IsUnique()
            .HasDatabaseName("IX_UserSettings_TenantId_UserId");

        builder.HasIndex(us => us.UserId)
            .IsUnique()
            .HasDatabaseName("IX_UserSettings_UserId");
    }
}
