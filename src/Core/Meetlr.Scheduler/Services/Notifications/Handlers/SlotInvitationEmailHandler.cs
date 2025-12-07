using System.Text.Json;
using MediatR;
using Meetlr.Application.Common.Interfaces;
using Meetlr.Application.Common.Settings;
using Meetlr.Module.Notifications.Application.Commands.SendEmailNotification;
using Meetlr.Application.Features.Notifications.Models;
using Meetlr.Application.Interfaces;
using Meetlr.Domain.Entities.Events;
using Meetlr.Domain.Entities.Notifications;
using Meetlr.Domain.Enums;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace Meetlr.Scheduler.Services.Notifications.Handlers;

/// <summary>
/// Handler for sending slot invitation emails.
/// Fetches invitation details and sends a templated email to the invitee.
/// </summary>
public class SlotInvitationEmailHandler : INotificationTypeHandler
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IEmailTemplateRenderer _emailTemplateRenderer;
    private readonly ILogger<SlotInvitationEmailHandler> _logger;
    private readonly ApplicationUrlsSettings _urlSettings;

    public SlotInvitationEmailHandler(
        IUnitOfWork unitOfWork,
        IEmailTemplateRenderer emailTemplateRenderer,
        IOptions<ApplicationUrlsSettings> urlSettings,
        ILogger<SlotInvitationEmailHandler> logger)
    {
        _unitOfWork = unitOfWork;
        _emailTemplateRenderer = emailTemplateRenderer;
        _urlSettings = urlSettings.Value;
        _logger = logger;
    }

    public async Task<NotificationHandlerResult> HandleAsync(
        NotificationPending notification,
        NotificationPayload? payload,
        IMediator mediator,
        CancellationToken cancellationToken)
    {
        try
        {
            // Parse payload to get SlotInvitationId
            Guid slotInvitationId;
            if (!string.IsNullOrEmpty(notification.PayloadJson))
            {
                var payloadObj = JsonSerializer.Deserialize<SlotInvitationPayload>(notification.PayloadJson);
                slotInvitationId = payloadObj?.SlotInvitationId ?? Guid.Empty;
            }
            else
            {
                return new NotificationHandlerResult
                {
                    Success = false,
                    ErrorMessage = "No SlotInvitationId in payload"
                };
            }

            // Fetch the slot invitation with related data
            var invitation = await _unitOfWork.Repository<Meetlr.Module.SlotInvitation.Domain.Entities.SlotInvitation>()
                .GetQueryable()
                .AsNoTracking()
                .FirstOrDefaultAsync(si => si.Id == slotInvitationId, cancellationToken);

            if (invitation == null)
            {
                return new NotificationHandlerResult
                {
                    Success = false,
                    ErrorMessage = $"Slot invitation {slotInvitationId} not found"
                };
            }

            // Fetch the event type for details
            var meetlrEvent = await _unitOfWork.Repository<MeetlrEvent>()
                .GetQueryable()
                .AsNoTracking()
                .Include(e => e.User)
                .FirstOrDefaultAsync(e => e.Id == invitation.MeetlrEventId, cancellationToken);

            if (meetlrEvent == null)
            {
                return new NotificationHandlerResult
                {
                    Success = false,
                    ErrorMessage = $"MeetlrEvent {invitation.MeetlrEventId} not found"
                };
            }

            // Build the booking URL using the dedicated invite page
            var bookingUrl = $"{_urlSettings.BaseUrl}/invite/{invitation.Token}";

            // Format the slot time
            var slotTime = FormatSlotTime(invitation.SlotStartTime, invitation.SlotEndTime, meetlrEvent.MeetingType);

            // Build host full name
            var hostFullName = $"{meetlrEvent.User.FirstName} {meetlrEvent.User.LastName}".Trim();
            if (string.IsNullOrEmpty(hostFullName))
            {
                hostFullName = meetlrEvent.User.Email ?? "Host";
            }

            // Build template variables
            var variables = new Dictionary<string, object>
            {
                ["inviteeName"] = invitation.InviteeName ?? invitation.InviteeEmail,
                ["hostName"] = hostFullName,
                ["eventName"] = meetlrEvent.Name,
                ["slotTime"] = slotTime,
                ["duration"] = meetlrEvent.DurationMinutes.ToString(),
                ["expirationHours"] = invitation.ExpirationHours.ToString(),
                ["expiresAt"] = invitation.ExpiresAt.ToString("f"),
                ["bookingUrl"] = bookingUrl,
                ["hostEmail"] = meetlrEvent.User.Email ?? ""
            };

            // Get and render the email template
            var (subject, htmlBody, plainTextBody) = await _emailTemplateRenderer.RenderForEventAsync(
                EmailTemplateType.SlotInvitation,
                variables,
                meetlrEvent.Id,
                notification.TenantId,
                meetlrEvent.UserId,
                cancellationToken);

            // Send the email
            var command = new SendEmailNotificationCommand
            {
                NotificationPendingId = notification.Id,
                ToEmail = invitation.InviteeEmail,
                Subject = subject,
                HtmlBody = htmlBody,
                PlainTextBody = plainTextBody,
                Metadata = new Dictionary<string, string>
                {
                    ["SlotInvitationId"] = slotInvitationId.ToString(),
                    ["MeetlrEventId"] = invitation.MeetlrEventId.ToString()
                }
            };

            var result = await mediator.Send(command, cancellationToken);

            // Update the invitation's email status
            if (result.Success)
            {
                await UpdateInvitationEmailStatus(slotInvitationId, true, null, cancellationToken);
            }
            else
            {
                await UpdateInvitationEmailStatus(slotInvitationId, false, result.ErrorMessage, cancellationToken);
            }

            return new NotificationHandlerResult
            {
                Success = result.Success,
                MessageId = result.MessageId,
                ErrorMessage = result.ErrorMessage
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error sending slot invitation email for notification {NotificationId}", notification.Id);
            return new NotificationHandlerResult
            {
                Success = false,
                ErrorMessage = ex.Message
            };
        }
    }

    private async Task UpdateInvitationEmailStatus(Guid invitationId, bool success, string? error, CancellationToken cancellationToken)
    {
        try
        {
            var invitation = await _unitOfWork.Repository<Meetlr.Module.SlotInvitation.Domain.Entities.SlotInvitation>()
                .GetQueryable()
                .FirstOrDefaultAsync(si => si.Id == invitationId, cancellationToken);

            if (invitation != null)
            {
                invitation.UpdateEmailStatus(success, error);
                await _unitOfWork.SaveChangesAsync(cancellationToken);
            }
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Failed to update email status for slot invitation {InvitationId}", invitationId);
            // Don't throw - email was sent, status update is secondary
        }
    }

    private static string FormatSlotTime(DateTime startTime, DateTime endTime, MeetingType meetingType)
    {
        if (meetingType == MeetingType.FullDay)
        {
            return startTime.ToString("dddd, MMMM d, yyyy") + " (Full Day)";
        }

        // Check if same day
        if (startTime.Date == endTime.Date)
        {
            return $"{startTime:dddd, MMMM d, yyyy} at {startTime:h:mm tt} - {endTime:h:mm tt}";
        }

        return $"{startTime:dddd, MMMM d, yyyy h:mm tt} - {endTime:dddd, MMMM d, yyyy h:mm tt}";
    }

    private class SlotInvitationPayload
    {
        public Guid SlotInvitationId { get; set; }
    }
}
