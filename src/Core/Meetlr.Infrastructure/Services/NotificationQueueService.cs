using System.Text.Json;
using Meetlr.Domain.Entities.Events;
using Meetlr.Application.Common.Interfaces;
using Meetlr.Application.Interfaces;
using Meetlr.Domain.Entities.Notifications;
using Meetlr.Domain.Enums;
using Microsoft.Extensions.Logging;
using Meetlr.Infrastructure.Services.Notifications;

namespace Meetlr.Infrastructure.Services;

/// <summary>
/// Service for queuing notifications - acts as a facade over specialized notification services
/// Refactored to use composition and SRP (Single Responsibility Principle)
/// </summary>
public class NotificationQueueService : INotificationQueueService
{
    private readonly SingleBookingNotificationService _singleBookingService;
    private readonly RecurringBookingsNotificationService _recurringBookingsService;
    private readonly IUnitOfWork _unitOfWork;
    private readonly ILogger<NotificationQueueService> _logger;

    public NotificationQueueService(
        SingleBookingNotificationService singleBookingService,
        RecurringBookingsNotificationService recurringBookingsService,
        IUnitOfWork unitOfWork,
        ILogger<NotificationQueueService> logger)
    {
        _singleBookingService = singleBookingService;
        _recurringBookingsService = recurringBookingsService;
        _unitOfWork = unitOfWork;
        _logger = logger;
    }

    /// <summary>
    /// Queue notifications for a booking based on event type preferences
    /// Delegates to SingleBookingNotificationService
    /// </summary>
    public async Task QueueBookingNotificationsAsync(
        Booking booking,
        MeetlrEvent meetlrEvent,
        NotificationTrigger trigger,
        CancellationToken cancellationToken = default)
    {
        await _singleBookingService.QueueBookingNotificationsAsync(booking, meetlrEvent, trigger, cancellationToken);
    }

    /// <summary>
    /// Queue a single notification without content (content will be built by command handlers)
    /// </summary>
    public async Task QueueNotificationAsync(
        Guid bookingId,
        Guid meetlrEventId,
        Guid userId,
        NotificationType notificationType,
        NotificationTrigger trigger,
        string recipient,
        DateTime? scheduledAt = null,
        DateTime? executeAt = null,
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
            ScheduledAt = scheduledAt ?? DateTime.UtcNow,
            ExecuteAt = executeAt ?? scheduledAt ?? DateTime.UtcNow,
            MaxRetries = 3,
            RetryCount = 0,
            CreatedAt = DateTime.UtcNow
        };

        _unitOfWork.Repository<NotificationPending>().Add(notification);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        _logger.LogInformation(
            "Queued {NotificationType} notification {NotificationId} for {Recipient}, trigger {Trigger}, scheduled for {ScheduledAt}, execute at {ExecuteAt}",
            notificationType,
            notification.Id,
            recipient,
            trigger,
            notification.ScheduledAt,
            notification.ExecuteAt);
    }

    /// <summary>
    /// Queue notifications for a recurring booking series
    /// Delegates to RecurringBookingsNotificationService
    /// </summary>
    public async Task QueueSeriesNotificationsAsync(
        BookingSeries series,
        List<Booking> bookings,
        MeetlrEvent eventType,
        CancellationToken cancellationToken = default)
    {
        await _recurringBookingsService.QueueSeriesNotificationsAsync(series, bookings, eventType, cancellationToken);
    }

    /// <summary>
    /// Queue a refund for processing by the background service
    /// No payload needed - the handler fetches payment details from Booking via BookingId
    /// </summary>
    public async Task QueueRefundAsync(
        Booking booking,
        CancellationToken cancellationToken = default)
    {
        var notification = new NotificationPending
        {
            Id = Guid.NewGuid(),
            BookingId = booking.Id,
            MeetlrEventId = booking.MeetlrEventId,
            UserId = booking.HostUserId,
            NotificationType = NotificationType.Refund,
            Trigger = NotificationTrigger.BookingCancelled,
            Status = NotificationStatus.Pending,
            Recipient = booking.Contact?.Email ?? "refund", // For logging/tracking purposes
            PayloadJson = string.Empty, // No payload needed - fetches from Booking
            ScheduledAt = DateTime.UtcNow,
            ExecuteAt = DateTime.UtcNow, // Process immediately
            MaxRetries = 5, // 5 retries for refunds as per user requirement
            RetryCount = 0,
            CreatedAt = DateTime.UtcNow
        };

        _unitOfWork.Repository<NotificationPending>().Add(notification);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        _logger.LogInformation(
            "Queued refund for booking {BookingId}, PaymentIntentId: {PaymentIntentId}, Amount: {Amount}",
            booking.Id,
            booking.PaymentIntentId,
            booking.Amount);
    }

    /// <summary>
    /// Queue a task (non-notification) for processing by the background service.
    /// Used for VideoLinkCreation, CalendarSync, VideoLinkDeletion, CalendarDeletion.
    /// </summary>
    public async Task QueueTaskAsync(
        Guid bookingId,
        Guid meetlrEventId,
        Guid userId,
        Guid tenantId,
        NotificationType taskType,
        DateTime? executeAt = null,
        CancellationToken cancellationToken = default)
    {
        var notification = new NotificationPending
        {
            Id = Guid.NewGuid(),
            TenantId = tenantId,
            BookingId = bookingId,
            MeetlrEventId = meetlrEventId,
            UserId = userId,
            NotificationType = taskType,
            Trigger = NotificationTrigger.BookingCreated, // Not used for tasks, but required field
            Status = NotificationStatus.Pending,
            Recipient = "task", // Not used for tasks - just for tracking
            PayloadJson = string.Empty,
            ScheduledAt = executeAt ?? DateTime.UtcNow,
            ExecuteAt = executeAt ?? DateTime.UtcNow,
            MaxRetries = 3,
            RetryCount = 0,
            CreatedAt = DateTime.UtcNow
        };

        _unitOfWork.Repository<NotificationPending>().Add(notification);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        _logger.LogInformation(
            "Queued {TaskType} task for booking {BookingId}, execute at {ExecuteAt}",
            taskType,
            bookingId,
            notification.ExecuteAt);
    }

    /// <summary>
    /// Queue reschedule notification email with old and new time details.
    /// Stores old times in payload for the email handler to use.
    /// </summary>
    public async Task QueueRescheduleNotificationAsync(
        Booking booking,
        MeetlrEvent meetlrEvent,
        DateTime oldStartTime,
        DateTime oldEndTime,
        CancellationToken cancellationToken = default)
    {
        // Create payload with old times for the email handler
        var payload = new
        {
            OldStartTime = oldStartTime,
            OldEndTime = oldEndTime,
            StartTime = booking.StartTime,
            EndTime = booking.EndTime
        };
        var payloadJson = JsonSerializer.Serialize(payload);

        // Queue email notification
        if (!string.IsNullOrEmpty(booking.Contact?.Email) && meetlrEvent.SendConfirmationEmail)
        {
            var notification = new NotificationPending
            {
                Id = Guid.NewGuid(),
                TenantId = booking.TenantId,
                BookingId = booking.Id,
                MeetlrEventId = meetlrEvent.Id,
                UserId = meetlrEvent.UserId,
                NotificationType = NotificationType.Email,
                Trigger = NotificationTrigger.BookingRescheduled,
                Status = NotificationStatus.Pending,
                Recipient = booking.Contact.Email,
                PayloadJson = payloadJson,
                ScheduledAt = DateTime.UtcNow,
                ExecuteAt = DateTime.UtcNow.AddSeconds(5), // Slight delay to ensure calendar update completes first
                MaxRetries = 3,
                RetryCount = 0,
                CreatedAt = DateTime.UtcNow
            };

            _unitOfWork.Repository<NotificationPending>().Add(notification);
            await _unitOfWork.SaveChangesAsync(cancellationToken);

            _logger.LogInformation(
                "Queued reschedule email notification for booking {BookingId}, old time: {OldStartTime}, new time: {NewStartTime}",
                booking.Id,
                oldStartTime,
                booking.StartTime);
        }
    }

    /// <summary>
    /// Queue a slot invitation email to invite someone to book a specific time slot.
    /// </summary>
    public async Task QueueSlotInvitationEmailAsync(
        Guid slotInvitationId,
        Guid meetlrEventId,
        Guid userId,
        Guid tenantId,
        string recipientEmail,
        CancellationToken cancellationToken = default)
    {
        // Store the SlotInvitationId in the payload so the handler can fetch details
        var payload = new { SlotInvitationId = slotInvitationId };
        var payloadJson = JsonSerializer.Serialize(payload);

        var notification = new NotificationPending
        {
            Id = Guid.NewGuid(),
            TenantId = tenantId,
            BookingId = null, // No booking yet - this is an invitation
            MeetlrEventId = meetlrEventId,
            UserId = userId,
            NotificationType = NotificationType.SlotInvitationEmail,
            Trigger = NotificationTrigger.BookingCreated, // Reusing trigger - not directly applicable
            Status = NotificationStatus.Pending,
            Recipient = recipientEmail,
            PayloadJson = payloadJson,
            ScheduledAt = DateTime.UtcNow,
            ExecuteAt = DateTime.UtcNow, // Send immediately
            MaxRetries = 3,
            RetryCount = 0,
            CreatedAt = DateTime.UtcNow
        };

        _unitOfWork.Repository<NotificationPending>().Add(notification);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        _logger.LogInformation(
            "Queued slot invitation email for invitation {SlotInvitationId} to {Recipient}",
            slotInvitationId,
            recipientEmail);
    }
}
