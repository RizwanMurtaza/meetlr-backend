using System.Text.Json;
using MediatR;
using Meetlr.Application.Common.Interfaces;
using Meetlr.Application.Features.Notifications.Models;
using Meetlr.Application.Plugins.Services;
using Meetlr.Domain.Entities.Notifications;
using Meetlr.Domain.Enums;
using Meetlr.Scheduler.Services.Notifications.Handlers;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace Meetlr.Scheduler.Services.Notifications;

/// <summary>
/// Service responsible for processing pending notifications.
/// Handles fetching, processing, and managing notification lifecycle.
/// </summary>
public class NotificationProcessingService : INotificationProcessingService
{
    private readonly IServiceProvider _serviceProvider;
    private readonly ILogger<NotificationProcessingService> _logger;
    private readonly int _batchSize = 20;
    private readonly int _stuckThresholdMinutes = 5;

    public NotificationProcessingService(
        IServiceProvider serviceProvider,
        ILogger<NotificationProcessingService> logger)
    {
        _serviceProvider = serviceProvider;
        _logger = logger;
    }

    public async Task ProcessPendingNotificationsAsync(CancellationToken cancellationToken)
    {
        var pendingIds = await GetPendingNotificationIdsAsync(cancellationToken);

        if (!pendingIds.Any())
        {
            _logger.LogDebug("No pending notifications to process");
            return;
        }

        _logger.LogInformation("Processing {Count} pending notifications", pendingIds.Count);

        foreach (var notificationId in pendingIds)
        {
            if (cancellationToken.IsCancellationRequested)
                break;

            await ProcessSingleNotificationAsync(notificationId, cancellationToken);
        }
    }

    private async Task<List<Guid>> GetPendingNotificationIdsAsync(CancellationToken cancellationToken)
    {
        using var scope = _serviceProvider.CreateScope();
        var unitOfWork = scope.ServiceProvider.GetRequiredService<IUnitOfWork>();
        var repository = unitOfWork.Repository<NotificationPending>();

        var now = DateTime.UtcNow;
        var stuckThreshold = now.AddMinutes(-_stuckThresholdMinutes);

        // Recover stuck notifications first
        await RecoverStuckNotificationsAsync(scope, unitOfWork, now, stuckThreshold, cancellationToken);

        // Get pending notification IDs (excluding those that exceeded max retries)
        return await repository.GetQueryable()
            .AsNoTracking()
            .Where(n =>
                (n.Status == NotificationStatus.Pending || n.Status == NotificationStatus.Failed) &&
                n.ExecuteAt <= now &&
                n.RetryCount < n.MaxRetries &&
                (n.NextRetryAt == null || n.NextRetryAt <= now))
            .OrderBy(n => n.ExecuteAt)
            .Take(_batchSize)
            .Select(n => n.Id)
            .ToListAsync(cancellationToken);
    }

    private async Task RecoverStuckNotificationsAsync(
        IServiceScope scope,
        IUnitOfWork unitOfWork,
        DateTime now,
        DateTime stuckThreshold,
        CancellationToken cancellationToken)
    {
        var repository = unitOfWork.Repository<NotificationPending>();
        var historyManager = scope.ServiceProvider.GetRequiredService<INotificationHistoryManager>();
        var billingService = scope.ServiceProvider.GetService<INotificationBillingService>();

        var stuckNotifications = await repository.GetQueryable()
            .Where(n =>
                n.Status == NotificationStatus.Processing &&
                n.ProcessingStartedAt < stuckThreshold)
            .ToListAsync(cancellationToken);

        if (!stuckNotifications.Any())
            return;

        _logger.LogWarning("Found {Count} stuck notifications in Processing state", stuckNotifications.Count);

        foreach (var stuck in stuckNotifications)
        {
            stuck.RetryCount++;

            if (stuck.RetryCount >= stuck.MaxRetries)
            {
                _logger.LogWarning(
                    "Stuck notification {NotificationId} exceeded max retries, moving to history",
                    stuck.Id);

                // Refund any reserved credits before moving to history
                if (billingService != null)
                {
                    try
                    {
                        await billingService.RefundCreditsAsync(stuck.Id, cancellationToken);
                        _logger.LogInformation(
                            "Refunded reserved credits for stuck notification {NotificationId}",
                            stuck.Id);
                    }
                    catch (Exception ex)
                    {
                        _logger.LogError(ex,
                            "Failed to refund credits for stuck notification {NotificationId}",
                            stuck.Id);
                    }
                }

                // Move directly to history - don't leave as MaxRetriesExceeded in pending
                await historyManager.MoveToHistoryAsync(
                    stuck,
                    NotificationStatus.MaxRetriesExceeded,
                    null,
                    "Processing timed out - max retries exceeded",
                    stuck.ProcessingStartedAt ?? now,
                    cancellationToken);
            }
            else
            {
                stuck.Status = NotificationStatus.Failed;
                stuck.ErrorMessage = "Processing timed out - notification was stuck";
                stuck.NextRetryAt = now.AddMinutes(1);
                repository.Update(stuck);

                _logger.LogWarning(
                    "Reset stuck notification {NotificationId} to Failed, retry scheduled for {NextRetry}",
                    stuck.Id, stuck.NextRetryAt);
            }
        }

        // Only save if there were non-max-retry updates (history manager handles its own saves)
        if (stuckNotifications.Any(s => s.RetryCount < s.MaxRetries))
        {
            await unitOfWork.SaveChangesAsync(cancellationToken);
        }
    }

    private async Task ProcessSingleNotificationAsync(Guid notificationId, CancellationToken cancellationToken)
    {
        using var scope = _serviceProvider.CreateScope();
        var unitOfWork = scope.ServiceProvider.GetRequiredService<IUnitOfWork>();
        var mediator = scope.ServiceProvider.GetRequiredService<IMediator>();
        var handlerFactory = scope.ServiceProvider.GetRequiredService<INotificationHandlerFactory>();
        var historyManager = scope.ServiceProvider.GetRequiredService<INotificationHistoryManager>();

        try
        {
            var notification = await unitOfWork.Repository<NotificationPending>()
                .GetByIdAsync(notificationId, cancellationToken);

            if (notification == null)
            {
                _logger.LogWarning("Notification {NotificationId} not found, may have been processed already", notificationId);
                return;
            }

            if (!ShouldProcess(notification, notificationId))
                return;

            if (await HandleMaxRetriesExceeded(notification, historyManager, scope, cancellationToken))
                return;

            await MarkAsProcessingAsync(notification, unitOfWork, cancellationToken);
            await ExecuteHandlerAsync(notification, mediator, handlerFactory, historyManager, cancellationToken);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to process notification {NotificationId}", notificationId);
            await ResetToFailedAsync(notificationId, ex.Message, cancellationToken);
        }
    }

    private bool ShouldProcess(NotificationPending notification, Guid notificationId)
    {
        if (notification.Status != NotificationStatus.Pending && notification.Status != NotificationStatus.Failed)
        {
            _logger.LogDebug("Notification {NotificationId} status is {Status}, skipping", notificationId, notification.Status);
            return false;
        }
        return true;
    }

    private async Task<bool> HandleMaxRetriesExceeded(
        NotificationPending notification,
        INotificationHistoryManager historyManager,
        IServiceScope scope,
        CancellationToken cancellationToken)
    {
        if (notification.RetryCount < notification.MaxRetries)
            return false;

        _logger.LogWarning(
            "Notification {NotificationId} exceeded max retries ({RetryCount}/{MaxRetries}), moving to history",
            notification.Id, notification.RetryCount, notification.MaxRetries);

        // Refund any reserved credits for this notification
        // This handles the case where a notification got stuck in Processing state
        // and was marked as MaxRetriesExceeded by the recovery process
        var billingService = scope.ServiceProvider.GetService<INotificationBillingService>();
        if (billingService != null)
        {
            try
            {
                await billingService.RefundCreditsAsync(notification.Id, cancellationToken);
                _logger.LogInformation(
                    "Refunded reserved credits for notification {NotificationId} that exceeded max retries",
                    notification.Id);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex,
                    "Failed to refund credits for notification {NotificationId} that exceeded max retries",
                    notification.Id);
            }
        }

        await historyManager.MoveToHistoryAsync(
            notification,
            NotificationStatus.MaxRetriesExceeded,
            null,
            notification.ErrorMessage ?? "Max retries exceeded",
            DateTime.UtcNow,
            cancellationToken);

        return true;
    }

    private async Task MarkAsProcessingAsync(
        NotificationPending notification,
        IUnitOfWork unitOfWork,
        CancellationToken cancellationToken)
    {
        notification.Status = NotificationStatus.Processing;
        notification.ProcessingStartedAt = DateTime.UtcNow;
        unitOfWork.Repository<NotificationPending>().Update(notification);
        await unitOfWork.SaveChangesAsync(cancellationToken);
    }

    private async Task ExecuteHandlerAsync(
        NotificationPending notification,
        IMediator mediator,
        INotificationHandlerFactory handlerFactory,
        INotificationHistoryManager historyManager,
        CancellationToken cancellationToken)
    {
        var startTime = DateTime.UtcNow;

        _logger.LogInformation(
            "Processing notification {NotificationId} of type {Type} for recipient {Recipient}",
            notification.Id,
            notification.NotificationType,
            notification.Recipient);

        var handler = handlerFactory.GetHandler(notification.NotificationType);
        if (handler == null)
        {
            _logger.LogWarning(
                "Unknown notification type {Type} for notification {NotificationId}",
                notification.NotificationType,
                notification.Id);

            await historyManager.HandleFailureAsync(
                notification,
                $"Unknown notification type: {notification.NotificationType}",
                startTime,
                cancellationToken);
            return;
        }

        NotificationPayload? payload = null;
        if (!string.IsNullOrEmpty(notification.PayloadJson))
        {
            payload = JsonSerializer.Deserialize<NotificationPayload>(notification.PayloadJson);
        }

        var result = await handler.HandleAsync(notification, payload, mediator, cancellationToken);

        if (result.Success)
        {
            await historyManager.MoveToHistoryAsync(
                notification,
                NotificationStatus.Sent,
                result.MessageId,
                null,
                startTime,
                cancellationToken);
        }
        else
        {
            await historyManager.HandleFailureAsync(
                notification,
                result.ErrorMessage,
                startTime,
                cancellationToken);
        }
    }

    private async Task ResetToFailedAsync(Guid notificationId, string errorMessage, CancellationToken cancellationToken)
    {
        try
        {
            using var scope = _serviceProvider.CreateScope();
            var unitOfWork = scope.ServiceProvider.GetRequiredService<IUnitOfWork>();

            var notification = await unitOfWork.Repository<NotificationPending>()
                .GetByIdAsync(notificationId, cancellationToken);

            if (notification == null)
                return;

            notification.RetryCount++;
            notification.ErrorMessage = errorMessage;

            if (notification.RetryCount >= notification.MaxRetries)
            {
                notification.Status = NotificationStatus.MaxRetriesExceeded;
                _logger.LogWarning(
                    "Notification {NotificationId} exceeded max retries after error, marking as MaxRetriesExceeded",
                    notificationId);
            }
            else
            {
                notification.Status = NotificationStatus.Failed;
                notification.NextRetryAt = DateTime.UtcNow.AddMinutes(Math.Pow(2, notification.RetryCount));
                _logger.LogWarning(
                    "Reset notification {NotificationId} to Failed status, retry {RetryCount}/{MaxRetries}",
                    notification.Id,
                    notification.RetryCount,
                    notification.MaxRetries);
            }

            unitOfWork.Repository<NotificationPending>().Update(notification);
            await unitOfWork.SaveChangesAsync(cancellationToken);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to reset notification {NotificationId} status", notificationId);
        }
    }
}
