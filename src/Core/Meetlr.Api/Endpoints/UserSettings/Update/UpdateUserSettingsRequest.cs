using Meetlr.Application.Features.UserSettings.Commands.UpdateUserSettings;
using Meetlr.Domain.Enums;

namespace Meetlr.Api.Endpoints.UserSettings.Update;

public class UpdateUserSettingsRequest
{
    // Currency & Payments
    public string DefaultCurrency { get; set; } = null!;

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

    public UpdateUserSettingsCommand ToCommand(Guid userId)
    {
        return new UpdateUserSettingsCommand
        {
            UserId = userId,
            DefaultCurrency = DefaultCurrency,
            DefaultNotifyViaEmail = DefaultNotifyViaEmail,
            DefaultNotifyViaSms = DefaultNotifyViaSms,
            DefaultNotifyViaWhatsApp = DefaultNotifyViaWhatsApp,
            EmailNotificationsEnabled = EmailNotificationsEnabled,
            SmsNotificationsEnabled = SmsNotificationsEnabled,
            WhatsAppNotificationsEnabled = WhatsAppNotificationsEnabled,
            DefaultEventDuration = DefaultEventDuration,
            DefaultBufferBefore = DefaultBufferBefore,
            DefaultBufferAfter = DefaultBufferAfter,
            DefaultMinBookingNotice = DefaultMinBookingNotice,
            DefaultReminderHours = DefaultReminderHours,
            DefaultMeetingLocationType = DefaultMeetingLocationType,
            DefaultLocationDetails = DefaultLocationDetails,
            DefaultCalendarProviderId = DefaultCalendarProviderId,
            WeekStartsOn = WeekStartsOn,
            ProfileCompleted = ProfileCompleted,
            OnboardingStep = OnboardingStep,
            BookingPageTheme = BookingPageTheme,
            ShowPoweredBy = ShowPoweredBy,
            JobTitle = JobTitle,
            WebsiteUrl = WebsiteUrl,
            LinkedInUrl = LinkedInUrl,
            TwitterUrl = TwitterUrl
        };
    }
}
