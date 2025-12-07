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

namespace Meetlr.Module.Notifications.Application.Commands.SendBookingCancellationEmail;

/// <summary>
/// Handler for sending booking cancellation emails using well-designed templates
/// </summary>
public class SendBookingCancellationEmailCommandHandler : IRequestHandler<SendBookingCancellationEmailCommand, SendBookingCancellationEmailCommandResponse>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IMediator _mediator;
    private readonly IEmailTemplateRenderer _emailTemplateRenderer;
    private readonly ApplicationUrlsSettings _urlsSettings;
    private readonly ILogger<SendBookingCancellationEmailCommandHandler> _logger;

    public SendBookingCancellationEmailCommandHandler(
        IUnitOfWork unitOfWork,
        IMediator mediator,
        IEmailTemplateRenderer emailTemplateRenderer,
        IOptions<ApplicationUrlsSettings> urlsSettings,
        ILogger<SendBookingCancellationEmailCommandHandler> logger)
    {
        _unitOfWork = unitOfWork;
        _mediator = mediator;
        _emailTemplateRenderer = emailTemplateRenderer;
        _urlsSettings = urlsSettings.Value;
        _logger = logger;
    }

    public async Task<SendBookingCancellationEmailCommandResponse> Handle(
        SendBookingCancellationEmailCommand request,
        CancellationToken cancellationToken)
    {
        try
        {
            _logger.LogInformation(
                "Sending booking cancellation email for booking {BookingId}",
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
                _logger.LogWarning("Booking {BookingId} not found for cancellation email", request.BookingId);
                return new SendBookingCancellationEmailCommandResponse
                {
                    Success = false,
                    ErrorMessage = "Booking not found"
                };
            }

            // Build template variables
            var hoursUntil = (booking.StartTime - DateTime.UtcNow).TotalHours;
            var timeUntil = hoursUntil < 24 ? $"{Math.Round(hoursUntil)} hours" : $"{Math.Round(hoursUntil / 24)} days";

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
                ["bookingTime"] = booking.StartTime.ToString("h:mm tt"),
                ["bookingStartTime"] = booking.StartTime.ToString("yyyy-MM-dd HH:mm"),
                ["bookingEndTime"] = booking.EndTime.ToString("yyyy-MM-dd HH:mm"),
                ["timeUntilMeeting"] = timeUntil,
                ["location"] = booking.Location ?? "Not specified",
                ["meetingLink"] = booking.MeetingLink ?? "",
                ["notes"] = booking.Notes ?? "",
                ["cancellationReason"] = booking.CancellationReason ?? "",
                ["confirmationUrl"] = _urlsSettings.BuildBookingDetailsUrl(booking.ConfirmationToken),
                ["cancellationUrl"] = _urlsSettings.BuildCancellationUrl(booking.CancellationToken),
                ["tenantName"] = booking.HostUser.Tenant?.Name ?? "Calendly"
            };

            // Send to invitee (check for event-specific template first)
            var inviteeTemplateResult = await _emailTemplateRenderer.RenderForEventAsync(
                EmailTemplateType.BookingCancellationInvitee,
                variables,
                booking.MeetlrEventId,
                booking.HostUser.TenantId,
                null,
                cancellationToken);

            var (inviteeSubject, inviteeHtmlBody, _) = inviteeTemplateResult;

            await _mediator.Send(new SendEmailCommand
            {
                To = booking.Contact?.Email ?? string.Empty,
                Subject = inviteeSubject,
                Body = inviteeHtmlBody
            }, cancellationToken);

            // Send to host
            var hostTemplateResult = await _emailTemplateRenderer.RenderAsync(
                EmailTemplateType.BookingCancellationHost,
                variables,
                booking.HostUser.TenantId,
                booking.HostUserId,
                cancellationToken);

            var (hostSubject, hostHtmlBody, _) = hostTemplateResult;

            await _mediator.Send(new SendEmailCommand
            {
                To = booking.HostUser.Email!,
                Subject = hostSubject,
                Body = hostHtmlBody
            }, cancellationToken);

            _logger.LogInformation(
                "Booking cancellation email sent successfully for booking {BookingId}",
                request.BookingId);

            return new SendBookingCancellationEmailCommandResponse
            {
                Success = true
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex,
                "Failed to send booking cancellation email for booking {BookingId}",
                request.BookingId);

            return new SendBookingCancellationEmailCommandResponse
            {
                Success = false,
                ErrorMessage = ex.Message
            };
        }
    }
}
