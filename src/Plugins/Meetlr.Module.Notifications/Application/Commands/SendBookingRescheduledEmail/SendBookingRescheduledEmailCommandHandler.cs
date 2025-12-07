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

namespace Meetlr.Module.Notifications.Application.Commands.SendBookingRescheduledEmail;

/// <summary>
/// Handler for sending booking rescheduled emails using well-designed templates.
/// Shows old vs new time comparison.
/// </summary>
public class SendBookingRescheduledEmailCommandHandler : IRequestHandler<SendBookingRescheduledEmailCommand, SendBookingRescheduledEmailCommandResponse>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IMediator _mediator;
    private readonly IEmailTemplateRenderer _emailTemplateRenderer;
    private readonly ICalendarInviteGenerator _calendarInviteGenerator;
    private readonly ApplicationUrlsSettings _urlsSettings;
    private readonly ILogger<SendBookingRescheduledEmailCommandHandler> _logger;

    public SendBookingRescheduledEmailCommandHandler(
        IUnitOfWork unitOfWork,
        IMediator mediator,
        IEmailTemplateRenderer emailTemplateRenderer,
        ICalendarInviteGenerator calendarInviteGenerator,
        IOptions<ApplicationUrlsSettings> urlsSettings,
        ILogger<SendBookingRescheduledEmailCommandHandler> logger)
    {
        _unitOfWork = unitOfWork;
        _mediator = mediator;
        _emailTemplateRenderer = emailTemplateRenderer;
        _calendarInviteGenerator = calendarInviteGenerator;
        _urlsSettings = urlsSettings.Value;
        _logger = logger;
    }

    public async Task<SendBookingRescheduledEmailCommandResponse> Handle(
        SendBookingRescheduledEmailCommand request,
        CancellationToken cancellationToken)
    {
        try
        {
            _logger.LogInformation(
                "Sending booking rescheduled email for booking {BookingId}",
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
                _logger.LogWarning("Booking {BookingId} not found for rescheduled email", request.BookingId);
                return new SendBookingRescheduledEmailCommandResponse
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

                // Old booking times (for comparison)
                ["oldBookingDate"] = request.OldStartTime.ToString("dddd, MMMM dd, yyyy"),
                ["oldBookingTime"] = request.OldStartTime.ToString("dddd, MMMM dd, yyyy 'at' h:mm tt"),
                ["oldBookingStartTime"] = request.OldStartTime.ToString("yyyy-MM-dd HH:mm"),
                ["oldBookingEndTime"] = request.OldEndTime.ToString("yyyy-MM-dd HH:mm"),

                // New booking times
                ["bookingDate"] = booking.StartTime.ToString("dddd, MMMM dd, yyyy"),
                ["bookingTime"] = booking.StartTime.ToString("dddd, MMMM dd, yyyy 'at' h:mm tt"),
                ["bookingStartTime"] = booking.StartTime.ToString("yyyy-MM-dd HH:mm"),
                ["bookingEndTime"] = booking.EndTime.ToString("yyyy-MM-dd HH:mm"),
                ["newBookingDate"] = booking.StartTime.ToString("dddd, MMMM dd, yyyy"),
                ["newBookingTime"] = booking.StartTime.ToString("dddd, MMMM dd, yyyy 'at' h:mm tt"),
                ["newBookingStartTime"] = booking.StartTime.ToString("yyyy-MM-dd HH:mm"),
                ["newBookingEndTime"] = booking.EndTime.ToString("yyyy-MM-dd HH:mm"),

                ["timeUntilMeeting"] = timeUntil,
                ["timeUntil"] = timeUntil,
                ["location"] = formattedLocation,
                ["meetingLink"] = booking.MeetingLink ?? "",
                ["notes"] = booking.Notes ?? "",
                ["rescheduleCount"] = booking.RescheduleCount.ToString(),
                ["confirmationUrl"] = _urlsSettings.BuildBookingDetailsUrl(booking.ConfirmationToken),
                ["cancellationUrl"] = _urlsSettings.BuildCancellationUrl(booking.CancellationToken),
                ["rescheduleUrl"] = _urlsSettings.BuildRescheduleUrl(booking.Id, booking.ConfirmationToken),
                ["tenantName"] = booking.HostUser.Tenant?.Name ?? "Calendly"
            };

            // Render template for invitee
            var templateResult = await _emailTemplateRenderer.RenderForEventAsync(
                EmailTemplateType.BookingRescheduled,
                variables,
                booking.MeetlrEventId,
                booking.HostUser.TenantId,
                null,
                cancellationToken);

            var (subject, htmlBody, _) = templateResult;

            // Generate updated ICS calendar invite attachment
            var icsContent = _calendarInviteGenerator.GenerateIcsFile(new CalendarInviteRequest
            {
                EventUid = $"booking-{booking.Id}@meetlr.com",
                Title = $"{booking.MeetlrEvent.Name} with {booking.HostUser.FirstName} {booking.HostUser.LastName}",
                Description = booking.MeetlrEvent.Description,
                StartTimeUtc = booking.StartTime,
                EndTimeUtc = booking.EndTime,
                Location = formattedLocation,
                OrganizerEmail = booking.HostUser.Email ?? "",
                OrganizerName = $"{booking.HostUser.FirstName} {booking.HostUser.LastName}",
                AttendeeEmail = booking.Contact?.Email ?? "",
                AttendeeName = booking.Contact?.Name ?? "",
                MeetingUrl = booking.MeetingLink,
                Method = CalendarInviteMethod.Request,
                Sequence = booking.RescheduleCount // Increment sequence for updates
            });

            _logger.LogInformation(
                "Generated updated ICS calendar invite for rescheduled booking {BookingId}, size: {Size} bytes",
                booking.Id, icsContent.Length);

            var attachments = new List<EmailAttachment>
            {
                new EmailAttachment
                {
                    FileName = "invite.ics",
                    Content = icsContent,
                    ContentType = "text/calendar"
                }
            };

            // Send email to invitee using SendEmailCommand
            await _mediator.Send(new SendEmailCommand
            {
                To = booking.Contact?.Email ?? string.Empty,
                Subject = subject,
                Body = htmlBody,
                Attachments = attachments
            }, cancellationToken);

            _logger.LogInformation(
                "Booking rescheduled email sent successfully for booking {BookingId}",
                request.BookingId);

            return new SendBookingRescheduledEmailCommandResponse
            {
                Success = true
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex,
                "Failed to send booking rescheduled email for booking {BookingId}",
                request.BookingId);

            return new SendBookingRescheduledEmailCommandResponse
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
