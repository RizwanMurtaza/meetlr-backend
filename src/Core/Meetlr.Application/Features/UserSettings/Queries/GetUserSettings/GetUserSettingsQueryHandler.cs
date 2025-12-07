using MediatR;
using Meetlr.Application.Common.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace Meetlr.Application.Features.UserSettings.Queries.GetUserSettings;

public class GetUserSettingsQueryHandler : IRequestHandler<GetUserSettingsQuery, UserSettingsQueryResponse>
{
    private readonly IUnitOfWork _unitOfWork;

    public GetUserSettingsQueryHandler(IUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork;
    }

    public async Task<UserSettingsQueryResponse> Handle(GetUserSettingsQuery request, CancellationToken cancellationToken)
    {
        var settings = await _unitOfWork.Repository<Meetlr.Domain.Entities.Users.UserSettings>().GetQueryable()
            .FirstOrDefaultAsync(us => us.UserId == request.UserId, cancellationToken);

        // If user settings don't exist, create default settings
        if (settings == null)
        {
            settings = new Meetlr.Domain.Entities.Users.UserSettings
            {
                UserId = request.UserId
            };

            _unitOfWork.Repository<Meetlr.Domain.Entities.Users.UserSettings>().Add(settings);
            await _unitOfWork.SaveChangesAsync(cancellationToken);
        }

        return new UserSettingsQueryResponse
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
