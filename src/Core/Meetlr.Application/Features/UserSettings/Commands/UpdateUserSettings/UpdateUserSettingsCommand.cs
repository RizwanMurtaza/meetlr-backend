using MediatR;
using Meetlr.Domain.Enums;

namespace Meetlr.Application.Features.UserSettings.Commands.UpdateUserSettings;

public class UpdateUserSettingsCommand : IRequest<UserSettingsCommandResponse>
{
    public Guid UserId { get; set; }

    // Currency & Payments
    public string DefaultCurrency { get; set; } = "USD";

    // Notification Defaults
    public bool DefaultNotifyViaEmail { get; set; }
    public bool DefaultNotifyViaSms { get; set; }
    public bool DefaultNotifyViaWhatsApp { get; set; }

    // Notification Master Switches
    public bool EmailNotificationsEnabled { get; set; }
    public bool SmsNotificationsEnabled { get; set; }
    public bool WhatsAppNotificationsEnabled { get; set; }

    // Meeting Defaults
    public int DefaultEventDuration { get; set; }
    public int DefaultBufferBefore { get; set; }
    public int DefaultBufferAfter { get; set; }
    public int DefaultMinBookingNotice { get; set; }
    public int DefaultReminderHours { get; set; }

    // Location Defaults
    public MeetingLocationType DefaultMeetingLocationType { get; set; }
    public string? DefaultLocationDetails { get; set; }

    // Calendar Preferences
    public Guid? DefaultCalendarProviderId { get; set; }
    public WeekStartsOn WeekStartsOn { get; set; }

    // UX Settings
    public bool ProfileCompleted { get; set; }
    public int OnboardingStep { get; set; }
    public string? BookingPageTheme { get; set; }
    public bool ShowPoweredBy { get; set; }

    // Professional Info
    public string? JobTitle { get; set; }
    public string? WebsiteUrl { get; set; }
    public string? LinkedInUrl { get; set; }
    public string? TwitterUrl { get; set; }
}
