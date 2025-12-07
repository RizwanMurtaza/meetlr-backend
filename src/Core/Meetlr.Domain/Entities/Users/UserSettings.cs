using Meetlr.Domain.Common;
using Meetlr.Domain.Enums;

namespace Meetlr.Domain.Entities.Users;

public class UserSettings : BaseEntity
{
    // Foreign Key
    public Guid UserId { get; set; }

    // Navigation Property
    public User User { get; set; } = null!;

    // Currency & Payments
    public string DefaultCurrency { get; set; } = "USD";

    // Notification Defaults
    public bool DefaultNotifyViaEmail { get; set; } = true;
    public bool DefaultNotifyViaSms { get; set; } = false;
    public bool DefaultNotifyViaWhatsApp { get; set; } = false;

    // Notification Master Switches
    public bool EmailNotificationsEnabled { get; set; } = true;
    public bool SmsNotificationsEnabled { get; set; } = false;
    public bool WhatsAppNotificationsEnabled { get; set; } = false;

    // Meeting Defaults
    public int DefaultEventDuration { get; set; } = 30; // minutes
    public int DefaultBufferBefore { get; set; } = 0; // minutes
    public int DefaultBufferAfter { get; set; } = 0; // minutes
    public int DefaultMinBookingNotice { get; set; } = 60; // minutes (converted to hours in UI: 60 = 1 hour)
    public int DefaultMaxBookingDaysInFuture { get; set; } = 60; // days (Booking Window)
    public int DefaultSlotIntervalMinutes { get; set; } = 15; // minutes (Start Time Increments)
    public int DefaultReminderHours { get; set; } = 24; // hours

    public bool DefaultSendReminderEmail { get; set; } = true;

    // Timezone Display Mode: true = auto-detect invitee's timezone, false = lock to schedule's timezone
    public bool AutoDetectInviteeTimezone { get; set; } = true;
    
    // Follow-up Notification Settings
    public bool FollowUpEnabled { get; set; } = false;
    public int FollowUpHoursAfter { get; set; } = 24; // hours after meeting ends

    // Location Defaults
    public MeetingLocationType DefaultMeetingLocationType { get; set; } = MeetingLocationType.Zoom;
    public string? DefaultLocationDetails { get; set; }

    // Calendar Preferences
    public Guid? DefaultCalendarProviderId { get; set; }
    public WeekStartsOn WeekStartsOn { get; set; } = WeekStartsOn.Sunday;

    // UX Settings
    public bool ProfileCompleted { get; set; } = false;
    public int OnboardingStep { get; set; } = 0;
    public string? BookingPageTheme { get; set; } = "default";
    public bool ShowPoweredBy { get; set; } = true;

    // Professional Info
    public string? JobTitle { get; set; }
    public string? WebsiteUrl { get; set; }
    public string? LinkedInUrl { get; set; }
    public string? TwitterUrl { get; set; }
}
