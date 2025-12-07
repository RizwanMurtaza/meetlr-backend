using MediatR;
using Meetlr.Application.Common.Interfaces;
using Meetlr.Application.Interfaces;
using Meetlr.Application.Plugins.Services;
using Meetlr.Domain.Entities.Notifications;
using Microsoft.Extensions.Logging;

namespace Meetlr.Module.Notifications.Application.Commands.SendWhatsAppNotification;

/// <summary>
/// Handler responsible ONLY for sending WhatsApp notifications.
/// Does NOT manage pending/history tables - that's the background service's responsibility.
/// Uses reserve-then-confirm billing pattern to prevent double charging.
/// </summary>
public class SendWhatsAppNotificationCommandHandler : IRequestHandler<SendWhatsAppNotificationCommand, SendWhatsAppNotificationCommandResponse>
{
    private readonly IWhatsAppNotificationService _whatsAppService;
    private readonly IUnitOfWork _unitOfWork;
    private readonly INotificationBillingService? _billingService;
    private readonly ILogger<SendWhatsAppNotificationCommandHandler> _logger;

    public SendWhatsAppNotificationCommandHandler(
        IWhatsAppNotificationService whatsAppService,
        IUnitOfWork unitOfWork,
        ILogger<SendWhatsAppNotificationCommandHandler> logger,
        INotificationBillingService? billingService = null)
    {
        _whatsAppService = whatsAppService;
        _unitOfWork = unitOfWork;
        _logger = logger;
        _billingService = billingService;
    }

    public async Task<SendWhatsAppNotificationCommandResponse> Handle(
        SendWhatsAppNotificationCommand request,
        CancellationToken cancellationToken)
    {
        try
        {
            _logger.LogInformation(
                "Processing WhatsApp notification {NotificationId} to {Recipient}",
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

                return new SendWhatsAppNotificationCommandResponse
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
                    "WhatsApp",
                    request.NotificationPendingId,
                    request.PhoneNumber,
                    cancellationToken);

                if (!reservationResult.Success)
                {
                    _logger.LogWarning(
                        "Insufficient credits for WhatsApp notification {NotificationId} for user {UserId}",
                        request.NotificationPendingId,
                        pendingNotification.UserId);

                    return new SendWhatsAppNotificationCommandResponse
                    {
                        Success = false,
                        ErrorMessage = reservationResult.ErrorMessage ?? "Insufficient credits to send WhatsApp message"
                    };
                }

                if (reservationResult.AlreadyCharged)
                {
                    _logger.LogInformation(
                        "Credits already charged for WhatsApp notification {NotificationId}, this is a retry",
                        request.NotificationPendingId);
                }

                // If the notification was already sent successfully, don't send again
                if (reservationResult.AlreadySent)
                {
                    _logger.LogInformation(
                        "WhatsApp notification {NotificationId} was already sent successfully, skipping duplicate send",
                        request.NotificationPendingId);

                    return new SendWhatsAppNotificationCommandResponse
                    {
                        Success = true,
                        MessageId = "already-sent"
                    };
                }
            }

            // STEP 2: Send WhatsApp message
            var (success, messageId, errorMessage) = await _whatsAppService.SendWhatsAppAsync(
                request.PhoneNumber,
                request.Message,
                request.MediaUrl,
                cancellationToken);

            // STEP 3: Confirm or refund credits based on result
            if (_billingService != null)
            {
                if (success)
                {
                    await _billingService.ConfirmCreditsUsedAsync(request.NotificationPendingId, cancellationToken);
                    _logger.LogInformation(
                        "WhatsApp notification {NotificationId} sent successfully with MessageId {MessageId}",
                        request.NotificationPendingId,
                        messageId);
                }
                else
                {
                    await _billingService.RefundCreditsAsync(request.NotificationPendingId, cancellationToken);
                    _logger.LogError(
                        "Failed to send WhatsApp notification {NotificationId}: {Error}. Credits refunded.",
                        request.NotificationPendingId,
                        errorMessage);
                }
            }
            else if (success)
            {
                _logger.LogInformation(
                    "WhatsApp notification {NotificationId} sent successfully with MessageId {MessageId}",
                    request.NotificationPendingId,
                    messageId);
            }
            else
            {
                _logger.LogError(
                    "Failed to send WhatsApp notification {NotificationId}: {Error}",
                    request.NotificationPendingId,
                    errorMessage);
            }

            return new SendWhatsAppNotificationCommandResponse
            {
                Success = success,
                MessageId = messageId,
                ErrorMessage = errorMessage
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex,
                "Exception while processing WhatsApp notification {NotificationId}",
                request.NotificationPendingId);

            // Refund credits on exception
            if (_billingService != null)
            {
                try
                {
                    await _billingService.RefundCreditsAsync(request.NotificationPendingId, cancellationToken);
                    _logger.LogInformation(
                        "Refunded credits for WhatsApp notification {NotificationId} due to exception",
                        request.NotificationPendingId);
                }
                catch (Exception refundEx)
                {
                    _logger.LogError(refundEx,
                        "Failed to refund credits for WhatsApp notification {NotificationId}",
                        request.NotificationPendingId);
                }
            }

            return new SendWhatsAppNotificationCommandResponse
            {
                Success = false,
                ErrorMessage = ex.Message
            };
        }
    }
}
