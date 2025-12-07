using Meetlr.Application.Plugins.Services;
using Meetlr.Module.Billing.Application.Interfaces;
using Meetlr.Module.Billing.Domain.Enums;
using Microsoft.Extensions.Logging;

namespace Meetlr.Module.Billing.Infrastructure.Services;

/// <summary>
/// Implementation of INotificationBillingService that delegates to the CreditService.
/// Uses reserve-then-confirm pattern to prevent double charging on retries.
/// </summary>
public class NotificationBillingService : INotificationBillingService
{
    private readonly ICreditService _creditService;
    private readonly ILogger<NotificationBillingService> _logger;

    public NotificationBillingService(
        ICreditService creditService,
        ILogger<NotificationBillingService> logger)
    {
        _creditService = creditService;
        _logger = logger;
    }

    public async Task<CreditReservationResult> ReserveCreditsAsync(
        Guid userId,
        string notificationType,
        Guid notificationId,
        string recipient,
        CancellationToken cancellationToken = default)
    {
        var serviceType = MapToServiceType(notificationType);
        if (serviceType == null)
        {
            _logger.LogWarning("Unknown notification type: {NotificationType}, skipping billing", notificationType);
            return new CreditReservationResult { Success = true, AlreadyCharged = false, AlreadySent = false };
        }

        var (success, alreadyCharged, alreadySent) = await _creditService.ReserveCreditsAsync(
            userId,
            serviceType.Value,
            notificationId,
            "Notification",
            recipient,
            cancellationToken);

        if (!success)
        {
            _logger.LogWarning(
                "Failed to reserve credits for {NotificationType} notification {NotificationId} for user {UserId}",
                notificationType, notificationId, userId);

            return new CreditReservationResult
            {
                Success = false,
                AlreadyCharged = false,
                AlreadySent = false,
                ErrorMessage = $"Insufficient credits to send {notificationType}"
            };
        }

        return new CreditReservationResult
        {
            Success = true,
            AlreadyCharged = alreadyCharged,
            AlreadySent = alreadySent
        };
    }

    public async Task ConfirmCreditsUsedAsync(Guid notificationId, CancellationToken cancellationToken = default)
    {
        await _creditService.ConfirmCreditsUsedAsync(notificationId, cancellationToken);
    }

    public async Task RefundCreditsAsync(Guid notificationId, CancellationToken cancellationToken = default)
    {
        await _creditService.RefundCreditsAsync(notificationId, cancellationToken);
    }

    private static ServiceType? MapToServiceType(string notificationType)
    {
        return notificationType.ToLowerInvariant() switch
        {
            "email" => ServiceType.Email,
            "sms" => ServiceType.SMS,
            "whatsapp" => ServiceType.WhatsApp,
            _ => null
        };
    }
}
