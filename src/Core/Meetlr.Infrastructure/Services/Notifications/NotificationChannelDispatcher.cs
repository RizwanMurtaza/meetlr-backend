using Meetlr.Application.Common.Interfaces;
using Meetlr.Domain.Entities.Events;
using Meetlr.Domain.Entities.Notifications;
using Meetlr.Domain.Enums;
using Microsoft.Extensions.Logging;

namespace Meetlr.Infrastructure.Services.Notifications;

/// <summary>
/// Handles dispatching notifications across multiple channels (Email, SMS, WhatsApp)
/// Responsibility: Queue notifications to pending table for each enabled channel
/// </summary>
public class NotificationChannelDispatcher
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly ILogger<NotificationChannelDispatcher> _logger;

    public NotificationChannelDispatcher(
        IUnitOfWork unitOfWork,
        ILogger<NotificationChannelDispatcher> logger)
    {
        _unitOfWork = unitOfWork;
        _logger = logger;
    }

    /// <summary>
    /// Queue a notification to all enabled channels based on event type preferences
    /// </summary>
    public async Task<int> QueueToEnabledChannelsAsync(
        Guid bookingId,
        Guid meetlrEventId,
        Guid userId,
        NotificationTrigger trigger,
        MeetlrEvent meetlrEvent,
        string email,
        string? phone,
        DateTime scheduledAt,
        DateTime executeAt,
        CancellationToken cancellationToken = default)
    {
        var notificationsQueued = 0;

        // Email channel
        if (meetlrEvent.NotifyViaEmail)
        {
            await QueueNotificationAsync(
                bookingId,
                meetlrEventId,
                userId,
                NotificationType.Email,
                trigger,
                email,
                scheduledAt,
                executeAt,
                cancellationToken);
            notificationsQueued++;
        }

        // SMS channel
        if (meetlrEvent.NotifyViaSms && !string.IsNullOrEmpty(phone))
        {
            await QueueNotificationAsync(
                bookingId,
                meetlrEventId,
                userId,
                NotificationType.Sms,
                trigger,
                phone,
                scheduledAt,
                executeAt,
                cancellationToken);
            notificationsQueued++;
        }

        // WhatsApp channel
        if (meetlrEvent.NotifyViaWhatsApp && !string.IsNullOrEmpty(phone))
        {
            await QueueNotificationAsync(
                bookingId,
                meetlrEventId,
                userId,
                NotificationType.WhatsApp,
                trigger,
                phone,
                scheduledAt,
                executeAt,
                cancellationToken);
            notificationsQueued++;
        }

        return notificationsQueued;
    }

    /// <summary>
    /// Queue a single notification to the pending table
    /// </summary>
    private async Task QueueNotificationAsync(
        Guid bookingId,
        Guid meetlrEventId,
        Guid userId,
        NotificationType notificationType,
        NotificationTrigger trigger,
        string recipient,
        DateTime scheduledAt,
        DateTime executeAt,
        CancellationToken cancellationToken = default)
    {
        var notification = new NotificationPending
        {
            Id = Guid.NewGuid(),
            BookingId = bookingId,
            MeetlrEventId = meetlrEventId,
            UserId = userId,
            NotificationType = notificationType,
            Trigger = trigger,
            Status = NotificationStatus.Pending,
            Recipient = recipient,
            PayloadJson = string.Empty, // No payload needed - command handlers will build content
            ScheduledAt = scheduledAt,
            ExecuteAt = executeAt,
            MaxRetries = 3,
            RetryCount = 0,
            CreatedAt = DateTime.UtcNow
        };

        _unitOfWork.Repository<NotificationPending>().Add(notification);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        _logger.LogDebug(
            "Queued {NotificationType} notification {NotificationId} for {Recipient}, trigger {Trigger}, execute at {ExecuteAt}",
            notificationType,
            notification.Id,
            recipient,
            trigger,
            notification.ExecuteAt);
    }
}
