using MediatR;
using Meetlr.Application.Common.Interfaces;
using Meetlr.Application.Common.Settings;
using Meetlr.Application.Interfaces;
using Meetlr.Domain.Entities.Events;
using Meetlr.Domain.Enums;
using Meetlr.Module.Notifications.Application.Commands.SendEmail;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace Meetlr.Module.Notifications.Application.Commands.SendBookingReminderEmail;

/// <summary>
/// Handler for sending booking reminder emails using well-designed templates
/// </summary>
public class SendBookingReminderEmailCommandHandler : IRequestHandler<SendBookingReminderEmailCommand, SendBookingReminderEmailCommandResponse>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IMediator _mediator;
    private readonly IEmailTemplateRenderer _emailTemplateRenderer;
    private readonly ApplicationUrlsSettings _urlsSettings;
    private readonly ILogger<SendBookingReminderEmailCommandHandler> _logger;

    public SendBookingReminderEmailCommandHandler(
        IUnitOfWork unitOfWork,
        IMediator mediator,
        IEmailTemplateRenderer emailTemplateRenderer,
        IOptions<ApplicationUrlsSettings> urlsSettings,
        ILogger<SendBookingReminderEmailCommandHandler> logger)
    {
        _unitOfWork = unitOfWork;
        _mediator = mediator;
        _emailTemplateRenderer = emailTemplateRenderer;
        _urlsSettings = urlsSettings.Value;
        _logger = logger;
    }

    public async Task<SendBookingReminderEmailCommandResponse> Handle(
        SendBookingReminderEmailCommand request,
        CancellationToken cancellationToken)
    {
        try
        {
            _logger.LogInformation(
                "Sending booking reminder email for booking {BookingId}",
                request.BookingId);

            // Get booking with all details including Contact
            var booking = await _unitOfWork.Repository<Booking>().GetQueryable()
                .Include(b => b.MeetlrEvent)
                .Include(b => b.HostUser)
                    .ThenInclude(u => u.Tenant)
                .Include(b => b.Contact)
                .FirstOrDefaultAsync(b => b.Id == request.BookingId, cancellationToken);

            if (booking == null)
            {
                _logger.LogWarning("Booking {BookingId} not found for reminder email", request.BookingId);
                return new SendBookingReminderEmailCommandResponse
                {
                    Success = false,
                    ErrorMessage = "Booking not found"
                };
            }

            // Build template variables
            var hoursUntil = (booking.StartTime - DateTime.UtcNow).TotalHours;
            var timeUntil = hoursUntil < 24 ? $"{Math.Round(hoursUntil)} hours" : $"{Math.Round(hoursUntil / 24)} days";

            // Format location based on meeting type
            var formattedLocation = FormatLocation(booking.MeetlrEvent.MeetingLocationType, booking.Location);

            var variables = new Dictionary<string, object>
            {
                ["inviteeName"] = booking.Contact?.Name ?? string.Empty,
                ["inviteeEmail"] = booking.Contact?.Email ?? string.Empty,
                ["hostName"] = $"{booking.HostUser.FirstName} {booking.HostUser.LastName}",
                ["hostFirstName"] = booking.HostUser.FirstName,
                ["hostLastName"] = booking.HostUser.LastName,
                ["hostEmail"] = booking.HostUser.Email ?? "",
                ["eventName"] = booking.MeetlrEvent.Name,
                ["eventDescription"] = booking.MeetlrEvent.Description ?? "",
                ["eventDuration"] = booking.MeetlrEvent.DurationMinutes.ToString(),
                ["bookingDate"] = booking.StartTime.ToString("dddd, MMMM dd, yyyy"),
                ["bookingTime"] = booking.StartTime.ToString("dddd, MMMM dd, yyyy 'at' h:mm tt"),
                ["bookingStartTime"] = booking.StartTime.ToString("yyyy-MM-dd HH:mm"),
                ["bookingEndTime"] = booking.EndTime.ToString("yyyy-MM-dd HH:mm"),
                ["timeUntilMeeting"] = timeUntil,
                ["timeUntil"] = timeUntil,
                ["location"] = formattedLocation,
                ["meetingLink"] = booking.MeetingLink ?? "",
                ["notes"] = booking.Notes ?? "",
                ["cancellationReason"] = booking.CancellationReason ?? "",
                ["confirmationUrl"] = _urlsSettings.BuildBookingDetailsUrl(booking.ConfirmationToken),
                ["cancellationUrl"] = _urlsSettings.BuildCancellationUrl(booking.CancellationToken),
                ["tenantName"] = booking.HostUser.Tenant?.Name ?? "Calendly"
            };

            // Render template (check for event-specific template first)
            var templateResult = await _emailTemplateRenderer.RenderForEventAsync(
                EmailTemplateType.BookingReminder,
                variables,
                booking.MeetlrEventId,
                booking.HostUser.TenantId,
                null,
                cancellationToken);

            var (subject, htmlBody, _) = templateResult;

            // Send email using SendEmailCommand
            await _mediator.Send(new SendEmailCommand
            {
                To = booking.Contact?.Email ?? string.Empty,
                Subject = subject,
                Body = htmlBody
            }, cancellationToken);

            _logger.LogInformation(
                "Booking reminder email sent successfully for booking {BookingId}",
                request.BookingId);

            return new SendBookingReminderEmailCommandResponse
            {
                Success = true
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex,
                "Failed to send booking reminder email for booking {BookingId}",
                request.BookingId);

            return new SendBookingReminderEmailCommandResponse
            {
                Success = false,
                ErrorMessage = ex.Message
            };
        }
    }

    private static string FormatLocation(MeetingLocationType locationType, string? locationDetails)
    {
        if (string.IsNullOrWhiteSpace(locationDetails))
        {
            return locationType switch
            {
                MeetingLocationType.InPerson => "In-person meeting (location to be confirmed)",
                MeetingLocationType.Zoom => "Zoom meeting (link will be provided)",
                MeetingLocationType.GoogleMeet => "Google Meet (link will be provided)",
                MeetingLocationType.MicrosoftTeams => "Microsoft Teams (link will be provided)",
                MeetingLocationType.PhoneCall => "Phone call (number will be provided)",
                _ => "Location to be confirmed"
            };
        }

        return locationType switch
        {
            MeetingLocationType.InPerson => $"In-person: {locationDetails}",
            MeetingLocationType.Zoom => $"Zoom: {locationDetails}",
            MeetingLocationType.GoogleMeet => $"Google Meet: {locationDetails}",
            MeetingLocationType.MicrosoftTeams => $"Microsoft Teams: {locationDetails}",
            MeetingLocationType.PhoneCall => $"Phone: {locationDetails}",
            _ => locationDetails
        };
    }
}
