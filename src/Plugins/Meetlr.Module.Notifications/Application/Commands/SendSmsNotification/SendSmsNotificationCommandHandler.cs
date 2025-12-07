using MediatR;
using Meetlr.Application.Common.Interfaces;
using Meetlr.Application.Interfaces;
using Meetlr.Application.Plugins.Services;
using Meetlr.Domain.Entities.Notifications;
using Microsoft.Extensions.Logging;

namespace Meetlr.Module.Notifications.Application.Commands.SendSmsNotification;

/// <summary>
/// Handler responsible ONLY for sending SMS notifications.
/// Does NOT manage pending/history tables - that's the background service's responsibility.
/// Uses reserve-then-confirm billing pattern to prevent double charging.
/// </summary>
public class SendSmsNotificationCommandHandler : IRequestHandler<SendSmsNotificationCommand, SendSmsNotificationCommandResponse>
{
    private readonly ISmsNotificationService _smsService;
    private readonly IUnitOfWork _unitOfWork;
    private readonly INotificationBillingService? _billingService;
    private readonly ILogger<SendSmsNotificationCommandHandler> _logger;

    public SendSmsNotificationCommandHandler(
        ISmsNotificationService smsService,
        IUnitOfWork unitOfWork,
        ILogger<SendSmsNotificationCommandHandler> logger,
        INotificationBillingService? billingService = null)
    {
        _smsService = smsService;
        _unitOfWork = unitOfWork;
        _logger = logger;
        _billingService = billingService;
    }

    public async Task<SendSmsNotificationCommandResponse> Handle(
        SendSmsNotificationCommand request,
        CancellationToken cancellationToken)
    {
        try
        {
            _logger.LogInformation(
                "Processing SMS notification {NotificationId} to {Recipient}",
                request.NotificationPendingId,
                request.PhoneNumber);

            // Get the pending notification to get UserId
            var pendingNotification = await _unitOfWork.Repository<NotificationPending>()
                .GetByIdAsync(request.NotificationPendingId, cancellationToken);

            if (pendingNotification == null)
            {
                _logger.LogWarning(
                    "Notification {NotificationId} not found in pending table",
                    request.NotificationPendingId);

                return new SendSmsNotificationCommandResponse
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
                    "SMS",
                    request.NotificationPendingId,
                    request.PhoneNumber,
                    cancellationToken);

                if (!reservationResult.Success)
                {
                    _logger.LogWarning(
                        "Insufficient credits for SMS notification {NotificationId} for user {UserId}",
                        request.NotificationPendingId,
                        pendingNotification.UserId);

                    return new SendSmsNotificationCommandResponse
                    {
                        Success = false,
                        ErrorMessage = reservationResult.ErrorMessage ?? "Insufficient credits to send SMS"
                    };
                }

                if (reservationResult.AlreadyCharged)
                {
                    _logger.LogInformation(
                        "Credits already charged for SMS notification {NotificationId}, this is a retry",
                        request.NotificationPendingId);
                }

                // If the notification was already sent successfully, don't send again
                if (reservationResult.AlreadySent)
                {
                    _logger.LogInformation(
                        "SMS notification {NotificationId} was already sent successfully, skipping duplicate send",
                        request.NotificationPendingId);

                    return new SendSmsNotificationCommandResponse
                    {
                        Success = true,
                        MessageId = "already-sent"
                    };
                }
            }

            // STEP 2: Send SMS
            var (success, messageId, errorMessage) = await _smsService.SendSmsAsync(
                request.PhoneNumber,
                request.Message,
                cancellationToken);

            // STEP 3: Confirm or refund credits based on result
            if (_billingService != null)
            {
                if (success)
                {
                    await _billingService.ConfirmCreditsUsedAsync(request.NotificationPendingId, cancellationToken);
                    _logger.LogInformation(
                        "SMS notification {NotificationId} sent successfully with MessageId {MessageId}",
                        request.NotificationPendingId,
                        messageId);
                }
                else
                {
                    await _billingService.RefundCreditsAsync(request.NotificationPendingId, cancellationToken);
                    _logger.LogError(
                        "Failed to send SMS notification {NotificationId}: {Error}. Credits refunded.",
                        request.NotificationPendingId,
                        errorMessage);
                }
            }
            else if (success)
            {
                _logger.LogInformation(
                    "SMS notification {NotificationId} sent successfully with MessageId {MessageId}",
                    request.NotificationPendingId,
                    messageId);
            }
            else
            {
                _logger.LogError(
                    "Failed to send SMS notification {NotificationId}: {Error}",
                    request.NotificationPendingId,
                    errorMessage);
            }

            return new SendSmsNotificationCommandResponse
            {
                Success = success,
                MessageId = messageId,
                ErrorMessage = errorMessage
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex,
                "Exception while processing SMS notification {NotificationId}",
                request.NotificationPendingId);

            // Refund credits on exception
            if (_billingService != null)
            {
                try
                {
                    await _billingService.RefundCreditsAsync(request.NotificationPendingId, cancellationToken);
                    _logger.LogInformation(
                        "Refunded credits for SMS notification {NotificationId} due to exception",
                        request.NotificationPendingId);
                }
                catch (Exception refundEx)
                {
                    _logger.LogError(refundEx,
                        "Failed to refund credits for SMS notification {NotificationId}",
                        request.NotificationPendingId);
                }
            }

            return new SendSmsNotificationCommandResponse
            {
                Success = false,
                ErrorMessage = ex.Message
            };
        }
    }
}
