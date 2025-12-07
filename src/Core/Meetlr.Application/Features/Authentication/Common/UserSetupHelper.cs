using Meetlr.Application.Common.Interfaces;
using Meetlr.Domain.Entities.Scheduling;
using Meetlr.Domain.Entities.Users;
using Meetlr.Domain.Enums;

namespace Meetlr.Application.Features.Authentication.Common;

/// <summary>
/// Shared helper for user setup operations during registration and OAuth signup.
/// Creates default availability schedules and user settings.
/// </summary>
public static class UserSetupHelper
{
    /// <summary>
    /// Creates a default availability schedule with working hours (Mon-Fri 9AM-5PM)
    /// </summary>
    public static AvailabilitySchedule CreateDefaultAvailabilitySchedule(
        Guid userId,
        Guid tenantId,
        string timeZone,
        IUnitOfWork unitOfWork)
    {
        var defaultSchedule = new AvailabilitySchedule
        {
            Id = Guid.NewGuid(),
            UserId = userId,
            TenantId = tenantId,
            Name = "Working hours (default)",
            TimeZone = timeZone,
            IsDefault = true,
            ScheduleType = ScheduleType.Personal,
            MaxBookingsPerSlot = 1,
            CreatedAt = DateTime.UtcNow
        };

        unitOfWork.Repository<AvailabilitySchedule>().Add(defaultSchedule);

        // Add default weekly hours (Monday to Friday, 9 AM - 5 PM)
        var workingDays = new[]
        {
            DayOfWeekEnum.Monday,
            DayOfWeekEnum.Tuesday,
            DayOfWeekEnum.Wednesday,
            DayOfWeekEnum.Thursday,
            DayOfWeekEnum.Friday
        };

        foreach (var day in workingDays)
        {
            var weeklyHour = new WeeklyHours
            {
                Id = Guid.NewGuid(),
                AvailabilityScheduleId = defaultSchedule.Id,
                TenantId = tenantId,
                DayOfWeek = day,
                IsAvailable = true,
                StartTime = TimeSpan.FromHours(9),
                EndTime = TimeSpan.FromHours(17),
                CreatedAt = DateTime.UtcNow
            };

            unitOfWork.Repository<WeeklyHours>().Add(weeklyHour);
        }

        // Add unavailable days (Saturday and Sunday)
        var weekendDays = new[]
        {
            DayOfWeekEnum.Saturday,
            DayOfWeekEnum.Sunday
        };

        foreach (var day in weekendDays)
        {
            var weeklyHour = new WeeklyHours
            {
                Id = Guid.NewGuid(),
                AvailabilityScheduleId = defaultSchedule.Id,
                TenantId = tenantId,
                DayOfWeek = day,
                IsAvailable = false,
                StartTime = TimeSpan.Zero,
                EndTime = TimeSpan.Zero,
                CreatedAt = DateTime.UtcNow
            };

            unitOfWork.Repository<WeeklyHours>().Add(weeklyHour);
        }

        return defaultSchedule;
    }

    /// <summary>
    /// Creates default user settings with sensible defaults.
    /// Currency and timezone can be customized based on location detection.
    /// </summary>
    public static Domain.Entities.Users.UserSettings CreateDefaultUserSettings(
        Guid userId,
        Guid tenantId,
        string? defaultCurrency = null,
        string? timeZone = null,
        IUnitOfWork? unitOfWork = null)
    {
        var userSettings = new Domain.Entities.Users.UserSettings
        {
            Id = Guid.NewGuid(),
            UserId = userId,
            TenantId = tenantId,

            // Currency - use provided or default to USD
            DefaultCurrency = defaultCurrency ?? "USD",

            // Notification defaults
            DefaultNotifyViaEmail = true,
            DefaultNotifyViaSms = false,
            DefaultNotifyViaWhatsApp = false,
            EmailNotificationsEnabled = true,
            SmsNotificationsEnabled = false,
            WhatsAppNotificationsEnabled = false,

            // Meeting defaults
            DefaultEventDuration = 30,
            DefaultBufferBefore = 0,
            DefaultBufferAfter = 0,
            DefaultMinBookingNotice = 60,
            DefaultReminderHours = 24,
            DefaultSendReminderEmail = true,

            // Follow-up settings
            FollowUpEnabled = false,
            FollowUpHoursAfter = 24,

            // Location defaults
            DefaultMeetingLocationType = MeetingLocationType.Zoom,

            // Calendar preferences
            WeekStartsOn = WeekStartsOn.Sunday,

            // UX settings
            ProfileCompleted = false,
            OnboardingStep = 0,
            BookingPageTheme = "default",
            ShowPoweredBy = true,

            CreatedAt = DateTime.UtcNow
        };

        unitOfWork?.Repository<Domain.Entities.Users.UserSettings>().Add(userSettings);

        return userSettings;
    }

    /// <summary>
    /// Gets default currency based on timezone or country code.
    /// This is a simple mapping - can be enhanced with IP geolocation.
    /// </summary>
    public static string GetDefaultCurrencyForTimezone(string? timeZone)
    {
        if (string.IsNullOrEmpty(timeZone))
            return "USD";

        // Simple timezone to currency mapping
        return timeZone switch
        {
            var tz when tz.Contains("Europe/London") => "GBP",
            var tz when tz.Contains("Europe/") => "EUR",
            var tz when tz.Contains("Asia/Tokyo") => "JPY",
            var tz when tz.Contains("Asia/Shanghai") || tz.Contains("Asia/Hong_Kong") => "CNY",
            var tz when tz.Contains("Asia/Kolkata") || tz.Contains("Asia/Mumbai") => "INR",
            var tz when tz.Contains("Australia/") => "AUD",
            var tz when tz.Contains("America/Toronto") || tz.Contains("America/Vancouver") => "CAD",
            var tz when tz.Contains("America/Mexico") => "MXN",
            var tz when tz.Contains("America/Sao_Paulo") => "BRL",
            var tz when tz.Contains("Asia/Dubai") => "AED",
            var tz when tz.Contains("Asia/Singapore") => "SGD",
            _ => "USD"
        };
    }
}
