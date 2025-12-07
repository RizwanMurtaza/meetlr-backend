using Meetlr.Application.Features.UserSettings.Commands.UpdateUserSettings;
using Meetlr.Application.Features.UserSettings.Queries.GetUserSettings;
using Meetlr.Domain.Enums;

namespace Meetlr.Api.Endpoints.UserSettings.Get;

public class UserSettingsResponseModel
{
    public Guid Id { get; set; }
    public Guid UserId { get; set; }

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

    public static UserSettingsResponseModel FromQueryResponse(UserSettingsQueryResponse response)
    {
        return new UserSettingsResponseModel
        {
            Id = response.Id,
            UserId = response.UserId,
            DefaultCurrency = response.DefaultCurrency,
            DefaultNotifyViaEmail = response.DefaultNotifyViaEmail,
            DefaultNotifyViaSms = response.DefaultNotifyViaSms,
            DefaultNotifyViaWhatsApp = response.DefaultNotifyViaWhatsApp,
            EmailNotificationsEnabled = response.EmailNotificationsEnabled,
            SmsNotificationsEnabled = response.SmsNotificationsEnabled,
            WhatsAppNotificationsEnabled = response.WhatsAppNotificationsEnabled,
            DefaultEventDuration = response.DefaultEventDuration,
            DefaultBufferBefore = response.DefaultBufferBefore,
            DefaultBufferAfter = response.DefaultBufferAfter,
            DefaultMinBookingNotice = response.DefaultMinBookingNotice,
            DefaultReminderHours = response.DefaultReminderHours,
            DefaultMeetingLocationType = response.DefaultMeetingLocationType,
            DefaultLocationDetails = response.DefaultLocationDetails,
            DefaultCalendarProviderId = response.DefaultCalendarProviderId,
            WeekStartsOn = response.WeekStartsOn,
            ProfileCompleted = response.ProfileCompleted,
            OnboardingStep = response.OnboardingStep,
            BookingPageTheme = response.BookingPageTheme,
            ShowPoweredBy = response.ShowPoweredBy,
            JobTitle = response.JobTitle,
            WebsiteUrl = response.WebsiteUrl,
            LinkedInUrl = response.LinkedInUrl,
            TwitterUrl = response.TwitterUrl
        };
    }

    public static UserSettingsResponseModel FromCommandResponse(UserSettingsCommandResponse response)
    {
        return new UserSettingsResponseModel
        {
            Id = response.Id,
            UserId = response.UserId,
            DefaultCurrency = response.DefaultCurrency,
            DefaultNotifyViaEmail = response.DefaultNotifyViaEmail,
            DefaultNotifyViaSms = response.DefaultNotifyViaSms,
            DefaultNotifyViaWhatsApp = response.DefaultNotifyViaWhatsApp,
            EmailNotificationsEnabled = response.EmailNotificationsEnabled,
            SmsNotificationsEnabled = response.SmsNotificationsEnabled,
            WhatsAppNotificationsEnabled = response.WhatsAppNotificationsEnabled,
            DefaultEventDuration = response.DefaultEventDuration,
            DefaultBufferBefore = response.DefaultBufferBefore,
            DefaultBufferAfter = response.DefaultBufferAfter,
            DefaultMinBookingNotice = response.DefaultMinBookingNotice,
            DefaultReminderHours = response.DefaultReminderHours,
            DefaultMeetingLocationType = response.DefaultMeetingLocationType,
            DefaultLocationDetails = response.DefaultLocationDetails,
            DefaultCalendarProviderId = response.DefaultCalendarProviderId,
            WeekStartsOn = response.WeekStartsOn,
            ProfileCompleted = response.ProfileCompleted,
            OnboardingStep = response.OnboardingStep,
            BookingPageTheme = response.BookingPageTheme,
            ShowPoweredBy = response.ShowPoweredBy,
            JobTitle = response.JobTitle,
            WebsiteUrl = response.WebsiteUrl,
            LinkedInUrl = response.LinkedInUrl,
            TwitterUrl = response.TwitterUrl
        };
    }
}
