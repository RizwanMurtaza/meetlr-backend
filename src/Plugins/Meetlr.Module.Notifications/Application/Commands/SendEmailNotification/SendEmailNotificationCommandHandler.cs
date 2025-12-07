using System.Text.Json;
using MediatR;
using Meetlr.Application.Common.Interfaces;
using Meetlr.Application.Features.Notifications.Models;
using Meetlr.Application.Plugins.Services;
using Meetlr.Domain.Entities.Notifications;
using Meetlr.Domain.Enums;
using Meetlr.Module.Notifications.Application.Commands.SendBookingCancellationEmail;
using Meetlr.Module.Notifications.Application.Commands.SendBookingConfirmationEmail;
using Meetlr.Module.Notifications.Application.Commands.SendBookingReminderEmail;
using Meetlr.Module.Notifications.Application.Commands.SendBookingRescheduledEmail;
using Meetlr.Module.Notifications.Application.Commands.SendEmail;
using Microsoft.Extensions.Logging;

namespace Meetlr.Module.Notifications.Application.Commands.SendEmailNotification;

/// <summary>
/// Handler responsible ONLY for sending the email.
/// Does NOT manage pending/history tables - that's the background service's responsibility.
/// Uses reserve-then-confirm billing pattern to prevent double charging.
/// </summary>
public class SendEmailNotificationCommandHandler : IRequestHandler<SendEmailNotificationCommand, SendEmailNotificationCommandResponse>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IMediator _mediator;
    private readonly INotificationBillingService? _billingService;
    private readonly ILogger<SendEmailNotificationCommandHandler> _logger;

    public SendEmailNotificationCommandHandler(
        IUnitOfWork unitOfWork,
        IMediator mediator,
        ILogger<SendEmailNotificationCommandHandler> logger,
        INotificationBillingService? billingService = null)
    {
        _unitOfWork = unitOfWork;
        _mediator = mediator;
        _logger = logger;
        _billingService = billingService;
    }

    public async Task<SendEmailNotificationCommandResponse> Handle(
        SendEmailNotificationCommand request,
        CancellationToken cancellationToken)
    {
        NotificationPending? pendingNotification = null;

        try
        {
            _logger.LogInformation(
                "Processing email notification {NotificationId} to {Recipient}",
                request.NotificationPendingId,
                request.ToEmail);

            // Get the pending notification to determine trigger type
            pendingNotification = await _unitOfWork.Repository<NotificationPending>()
                .GetByIdAsync(request.NotificationPendingId, cancellationToken);

            if (pendingNotification == null)
            {
                _logger.LogWarning(
                    "Notification {NotificationId} not found in pending table",
                    request.NotificationPendingId);

                return new SendEmailNotificationCommandResponse
                {
                    Success = false,
                    ErrorMessage = "Notification not found"
                };
            }

            // STEP 1: Reserve credits BEFORE sending (idempotent - won't double charge)
            if (_billingService != null)
            {
                var reservationResult = await _billingService.ReserveCreditsAsync(
                    pendingNotification.UserId,
                    "Email",
                    request.NotificationPendingId,
                    request.ToEmail,
                    cancellationToken);

                if (!reservationResult.Success)
                {
                    _logger.LogWarning(
                        "Insufficient credits for email notification {NotificationId} for user {UserId}",
                        request.NotificationPendingId,
                        pendingNotification.UserId);

                    return new SendEmailNotificationCommandResponse
                    {
                        Success = false,
                        ErrorMessage = reservationResult.ErrorMessage ?? "Insufficient credits to send email"
                    };
                }

                if (reservationResult.AlreadyCharged)
                {
                    _logger.LogInformation(
                        "Credits already charged for email notification {NotificationId}, this is a retry",
                        request.NotificationPendingId);
                }

                // If the notification was already sent successfully, don't send again
                if (reservationResult.AlreadySent)
                {
                    _logger.LogInformation(
                        "Email notification {NotificationId} was already sent successfully, skipping duplicate send",
                        request.NotificationPendingId);

                    return new SendEmailNotificationCommandResponse
                    {
                        Success = true,
                        MessageId = "already-sent"
                    };
                }
            }

            // STEP 2: Send email using CQRS commands based on trigger type
            var sendSuccess = await SendEmailByTriggerTypeAsync(pendingNotification, request, cancellationToken);

            // STEP 3: Confirm or refund credits based on result
            if (_billingService != null)
            {
                if (sendSuccess)
                {
                    await _billingService.ConfirmCreditsUsedAsync(request.NotificationPendingId, cancellationToken);
                    _logger.LogInformation(
                        "Email notification {NotificationId} sent successfully",
                        request.NotificationPendingId);
                }
                else
                {
                    await _billingService.RefundCreditsAsync(request.NotificationPendingId, cancellationToken);
                    _logger.LogError(
                        "Failed to send email notification {NotificationId}. Credits refunded.",
                        request.NotificationPendingId);

                    return new SendEmailNotificationCommandResponse
                    {
                        Success = false,
                        ErrorMessage = "Failed to send email"
                    };
                }
            }
            else if (sendSuccess)
            {
                _logger.LogInformation(
                    "Email notification {NotificationId} sent successfully",
                    request.NotificationPendingId);
            }
            else
            {
                return new SendEmailNotificationCommandResponse
                {
                    Success = false,
                    ErrorMessage = "Failed to send email"
                };
            }

            return new SendEmailNotificationCommandResponse
            {
                Success = true,
                MessageId = Guid.NewGuid().ToString()
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex,
                "Exception while processing email notification {NotificationId}",
                request.NotificationPendingId);

            // Refund credits on exception
            if (_billingService != null && pendingNotification != null)
            {
                try
                {
                    await _billingService.RefundCreditsAsync(request.NotificationPendingId, cancellationToken);
                    _logger.LogInformation(
                        "Refunded credits for email notification {NotificationId} due to exception",
                        request.NotificationPendingId);
                }
                catch (Exception refundEx)
                {
                    _logger.LogError(refundEx,
                        "Failed to refund credits for email notification {NotificationId}",
                        request.NotificationPendingId);
                }
            }

            return new SendEmailNotificationCommandResponse
            {
                Success = false,
                ErrorMessage = ex.Message
            };
        }
    }

    private async Task<bool> SendEmailByTriggerTypeAsync(
        NotificationPending pendingNotification,
        SendEmailNotificationCommand request,
        CancellationToken cancellationToken)
    {
        if (pendingNotification.BookingId.HasValue && pendingNotification.BookingId.Value != Guid.Empty)
        {
            var bookingId = pendingNotification.BookingId.Value;
            switch (pendingNotification.Trigger)
            {
                case NotificationTrigger.BookingCreated:
                    var confirmResult = await _mediator.Send(new SendBookingConfirmationEmailCommand
                    {
                        BookingId = bookingId
                    }, cancellationToken);
                    return confirmResult.Success;

                case NotificationTrigger.BookingCancelled:
                    var cancelResult = await _mediator.Send(new SendBookingCancellationEmailCommand
                    {
                        BookingId = bookingId
                    }, cancellationToken);
                    return cancelResult.Success;

                case NotificationTrigger.BookingReminder:
                    var reminderResult = await _mediator.Send(new SendBookingReminderEmailCommand
                    {
                        BookingId = bookingId
                    }, cancellationToken);
                    return reminderResult.Success;

                case NotificationTrigger.BookingRescheduled:
                    NotificationPayload? rescheduledPayload = null;
                    if (!string.IsNullOrEmpty(pendingNotification.PayloadJson))
                    {
                        rescheduledPayload = JsonSerializer.Deserialize<NotificationPayload>(pendingNotification.PayloadJson);
                    }

                    var rescheduleResult = await _mediator.Send(new SendBookingRescheduledEmailCommand
                    {
                        BookingId = bookingId,
                        OldStartTime = rescheduledPayload?.OldStartTime ?? DateTime.MinValue,
                        OldEndTime = rescheduledPayload?.OldEndTime ?? DateTime.MinValue
                    }, cancellationToken);
                    return rescheduleResult.Success;

                default:
                    await _mediator.Send(new SendEmailCommand
                    {
                        To = request.ToEmail,
                        Subject = request.Subject,
                        Body = request.HtmlBody
                    }, cancellationToken);
                    return true;
            }
        }
        else
        {
            await _mediator.Send(new SendEmailCommand
            {
                To = request.ToEmail,
                Subject = request.Subject,
                Body = request.HtmlBody
            }, cancellationToken);
            return true;
        }
    }
}
