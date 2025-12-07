using MediatR;
using Meetlr.Application.Common.Interfaces;
using Meetlr.Application.Interfaces;
using Meetlr.Domain.Enums;
using Microsoft.EntityFrameworkCore;

namespace Meetlr.Application.Features.UserSettings.Commands.UpdateUserSettings;

public class UpdateUserSettingsCommandHandler : IRequestHandler<UpdateUserSettingsCommand, UserSettingsCommandResponse>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IAuditService _auditService;

    public UpdateUserSettingsCommandHandler(
        IUnitOfWork unitOfWork,
        IAuditService auditService)
    {
        _unitOfWork = unitOfWork;
        _auditService = auditService;
    }

    public async Task<UserSettingsCommandResponse> Handle(UpdateUserSettingsCommand request, CancellationToken cancellationToken)
    {
        var settings = await _unitOfWork.Repository<Meetlr.Domain.Entities.Users.UserSettings>().GetQueryable()
            .FirstOrDefaultAsync(us => us.UserId == request.UserId, cancellationToken);

        // If settings don't exist, create them
        if (settings == null)
        {
            settings = new Meetlr.Domain.Entities.Users.UserSettings
            {
                UserId = request.UserId
            };
            _unitOfWork.Repository<Meetlr.Domain.Entities.Users.UserSettings>().Add(settings);
        }

        var oldValues = new
        {
            settings.DefaultCurrency,
            settings.DefaultNotifyViaEmail,
            settings.DefaultNotifyViaSms,
            settings.DefaultNotifyViaWhatsApp,
            settings.EmailNotificationsEnabled,
            settings.SmsNotificationsEnabled,
            settings.WhatsAppNotificationsEnabled,
            settings.DefaultEventDuration,
            settings.DefaultBufferBefore,
            settings.DefaultBufferAfter,
            settings.DefaultMinBookingNotice,
            settings.DefaultReminderHours,
            settings.DefaultMeetingLocationType,
            settings.DefaultLocationDetails,
            settings.DefaultCalendarProviderId,
            settings.WeekStartsOn,
            settings.ProfileCompleted,
            settings.OnboardingStep,
            settings.BookingPageTheme,
            settings.ShowPoweredBy,
            settings.JobTitle,
            settings.WebsiteUrl,
            settings.LinkedInUrl,
            settings.TwitterUrl
        };

        // Update all properties
        settings.DefaultCurrency = request.DefaultCurrency;
        settings.DefaultNotifyViaEmail = request.DefaultNotifyViaEmail;
        settings.DefaultNotifyViaSms = request.DefaultNotifyViaSms;
        settings.DefaultNotifyViaWhatsApp = request.DefaultNotifyViaWhatsApp;
        settings.EmailNotificationsEnabled = request.EmailNotificationsEnabled;
        settings.SmsNotificationsEnabled = request.SmsNotificationsEnabled;
        settings.WhatsAppNotificationsEnabled = request.WhatsAppNotificationsEnabled;
        settings.DefaultEventDuration = request.DefaultEventDuration;
        settings.DefaultBufferBefore = request.DefaultBufferBefore;
        settings.DefaultBufferAfter = request.DefaultBufferAfter;
        settings.DefaultMinBookingNotice = request.DefaultMinBookingNotice;
        settings.DefaultReminderHours = request.DefaultReminderHours;
        settings.DefaultMeetingLocationType = request.DefaultMeetingLocationType;
        settings.DefaultLocationDetails = request.DefaultLocationDetails;
        settings.DefaultCalendarProviderId = request.DefaultCalendarProviderId;
        settings.WeekStartsOn = request.WeekStartsOn;
        settings.ProfileCompleted = request.ProfileCompleted;
        settings.OnboardingStep = request.OnboardingStep;
        settings.BookingPageTheme = request.BookingPageTheme;
        settings.ShowPoweredBy = request.ShowPoweredBy;
        settings.JobTitle = request.JobTitle;
        settings.WebsiteUrl = request.WebsiteUrl;
        settings.LinkedInUrl = request.LinkedInUrl;
        settings.TwitterUrl = request.TwitterUrl;

        await _unitOfWork.SaveChangesAsync(cancellationToken);

        // Log audit
        await _auditService.LogAsync(
            AuditEntityType.User,
            settings.Id.ToString(),
            AuditAction.Update,
            oldValues,
            settings,
            cancellationToken);

        return new UserSettingsCommandResponse
        {
            Id = settings.Id,
            UserId = settings.UserId,
            DefaultCurrency = settings.DefaultCurrency,
            DefaultNotifyViaEmail = settings.DefaultNotifyViaEmail,
            DefaultNotifyViaSms = settings.DefaultNotifyViaSms,
            DefaultNotifyViaWhatsApp = settings.DefaultNotifyViaWhatsApp,
            EmailNotificationsEnabled = settings.EmailNotificationsEnabled,
            SmsNotificationsEnabled = settings.SmsNotificationsEnabled,
            WhatsAppNotificationsEnabled = settings.WhatsAppNotificationsEnabled,
            DefaultEventDuration = settings.DefaultEventDuration,
            DefaultBufferBefore = settings.DefaultBufferBefore,
            DefaultBufferAfter = settings.DefaultBufferAfter,
            DefaultMinBookingNotice = settings.DefaultMinBookingNotice,
            DefaultReminderHours = settings.DefaultReminderHours,
            DefaultMeetingLocationType = settings.DefaultMeetingLocationType,
            DefaultLocationDetails = settings.DefaultLocationDetails,
            DefaultCalendarProviderId = settings.DefaultCalendarProviderId,
            WeekStartsOn = settings.WeekStartsOn,
            ProfileCompleted = settings.ProfileCompleted,
            OnboardingStep = settings.OnboardingStep,
            BookingPageTheme = settings.BookingPageTheme,
            ShowPoweredBy = settings.ShowPoweredBy,
            JobTitle = settings.JobTitle,
            WebsiteUrl = settings.WebsiteUrl,
            LinkedInUrl = settings.LinkedInUrl,
            TwitterUrl = settings.TwitterUrl
        };
    }
}
