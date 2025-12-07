using Meetlr.Domain.Entities.Events;
using Meetlr.Domain.Enums;

namespace Meetlr.Application.Interfaces;

/// <summary>
/// Service for queuing notifications to the pending table
/// Does NOT build email content - that's handled by CQRS command handlers
/// </summary>
public interface INotificationQueueService
{
    /// <summary>
    /// Queue notifications for a booking based on event type preferences
    /// </summary>
    Task QueueBookingNotificationsAsync(
        Booking booking,
        MeetlrEvent eventType,
        NotificationTrigger trigger,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Queue a single notification without content (content will be built by command handlers)
    /// </summary>
    Task QueueNotificationAsync(
        Guid bookingId,
        Guid MeetlrEventId,
        Guid userId,
        NotificationType notificationType,
        NotificationTrigger trigger,
        string recipient,
        DateTime? scheduledAt = null,
        DateTime? executeAt = null,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Queue notifications for a recurring booking series
    /// Sends ONE confirmation for the series and individual reminders per booking
    /// </summary>
    Task QueueSeriesNotificationsAsync(
        BookingSeries series,
        List<Booking> bookings,
        MeetlrEvent eventType,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Queue a refund for processing by the background service
    /// Uses NotificationType.Refund - no payload needed, fetches from Booking via BookingId
    /// </summary>
    Task QueueRefundAsync(
        Booking booking,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Queue a task (non-notification) for processing by the background service.
    /// Used for VideoLinkCreation, CalendarSync, VideoLinkDeletion, CalendarDeletion.
    /// No recipient needed - these are internal tasks, not notifications.
    /// </summary>
    Task QueueTaskAsync(
        Guid bookingId,
        Guid meetlrEventId,
        Guid userId,
        Guid tenantId,
        NotificationType taskType,
        DateTime? executeAt = null,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Queue reschedule notification email with old and new time details.
    /// Stores old times in payload for the email handler to use.
    /// </summary>
    Task QueueRescheduleNotificationAsync(
        Booking booking,
        MeetlrEvent meetlrEvent,
        DateTime oldStartTime,
        DateTime oldEndTime,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Queue a slot invitation email to invite someone to book a specific time slot.
    /// </summary>
    Task QueueSlotInvitationEmailAsync(
        Guid slotInvitationId,
        Guid meetlrEventId,
        Guid userId,
        Guid tenantId,
        string recipientEmail,
        CancellationToken cancellationToken = default);
}
