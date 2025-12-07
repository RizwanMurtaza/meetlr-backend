using Meetlr.Domain.Entities.Events;
using Meetlr.Domain.Enums;
using Microsoft.Extensions.Logging;

namespace Meetlr.Infrastructure.Services.Notifications;

/// <summary>
/// Handles queuing notifications for single bookings
/// Responsibility: Queue confirmation, reminder, and follow-up notifications for individual bookings
/// </summary>
public class SingleBookingNotificationService
{
    private readonly NotificationChannelDispatcher _channelDispatcher;
    private readonly ILogger<SingleBookingNotificationService> _logger;

    public SingleBookingNotificationService(
        NotificationChannelDispatcher channelDispatcher,
        ILogger<SingleBookingNotificationService> logger)
    {
        _channelDispatcher = channelDispatcher;
        _logger = logger;
    }

    /// <summary>
    /// Queue all notifications for a booking (confirmation, reminder, follow-up)
    /// </summary>
    public async Task<int> QueueBookingNotificationsAsync(
        Booking booking,
        MeetlrEvent meetlrEvent,
        NotificationTrigger trigger,
        CancellationToken cancellationToken = default)
    {
        _logger.LogInformation(
            "Queuing notifications for booking {BookingId}, trigger {Trigger}",
            booking.Id,
            trigger);

        var notificationsQueued = 0;

        // For BookingCreated trigger, queue all 3 notifications (Confirmation, Reminder, FollowUp)
        // For other triggers (Cancelled, Rescheduled), queue only the specific notification
        if (trigger == NotificationTrigger.BookingCreated)
        {
            notificationsQueued += await QueueConfirmationNotificationsAsync(booking, meetlrEvent, cancellationToken);
            notificationsQueued += await QueueReminderNotificationsAsync(booking, meetlrEvent, cancellationToken);
            notificationsQueued += await QueueFollowUpNotificationsAsync(booking, meetlrEvent, cancellationToken);
        }
        else
        {
            // For other triggers (Cancelled, Rescheduled), queue single notification
            var scheduledAt = DateTime.UtcNow;
            var executeAt = DateTime.UtcNow; // Execute immediately

            notificationsQueued += await _channelDispatcher.QueueToEnabledChannelsAsync(
                booking.Id,
                meetlrEvent.Id,
                meetlrEvent.UserId,
                trigger,
                meetlrEvent,
                booking.Contact?.Email ?? string.Empty,
                booking.Contact?.Phone,
                scheduledAt,
                executeAt,
                cancellationToken);
        }

        _logger.LogInformation(
            "Queued {Count} notifications for booking {BookingId}",
            notificationsQueued,
            booking.Id);

        return notificationsQueued;
    }

    /// <summary>
    /// Queue confirmation notifications (sent immediately)
    /// </summary>
    private async Task<int> QueueConfirmationNotificationsAsync(
        Booking booking,
        MeetlrEvent meetlrEvent,
        CancellationToken cancellationToken)
    {
        var scheduledAt = DateTime.UtcNow;
        var executeAt = DateTime.UtcNow;

        return await _channelDispatcher.QueueToEnabledChannelsAsync(
            booking.Id,
            meetlrEvent.Id,
            meetlrEvent.UserId,
            NotificationTrigger.BookingCreated,
            meetlrEvent,
            booking.Contact?.Email ?? string.Empty,
            booking.Contact?.Phone,
            scheduledAt,
            executeAt,
            cancellationToken);
    }

    /// <summary>
    /// Queue reminder notifications (sent before meeting)
    /// </summary>
    private async Task<int> QueueReminderNotificationsAsync(
        Booking booking,
        MeetlrEvent meetlrEvent,
        CancellationToken cancellationToken)
    {
        if (!meetlrEvent.SendReminderEmail || meetlrEvent.ReminderHoursBefore <= 0)
            return 0;

        var scheduledAt = DateTime.UtcNow; // Created now
        var executeAt = booking.StartTime.AddHours(-meetlrEvent.ReminderHoursBefore); // Executed later

        // Skip queuing if the reminder time has already passed (booking is too soon)
        if (executeAt <= DateTime.UtcNow)
        {
            _logger.LogInformation(
                "Skipping reminder for booking {BookingId} - meeting starts in less than {Hours} hours",
                booking.Id,
                meetlrEvent.ReminderHoursBefore);
            return 0;
        }

        return await _channelDispatcher.QueueToEnabledChannelsAsync(
            booking.Id,
            meetlrEvent.Id,
            meetlrEvent.UserId,
            NotificationTrigger.BookingReminder,
            meetlrEvent,
            booking.Contact?.Email ?? string.Empty,
            booking.Contact?.Phone,
            scheduledAt,
            executeAt,
            cancellationToken);
    }

    /// <summary>
    /// Queue follow-up notifications (sent after meeting)
    /// </summary>
    private async Task<int> QueueFollowUpNotificationsAsync(
        Booking booking,
        MeetlrEvent meetlrEvent,
        CancellationToken cancellationToken)
    {
        if (!meetlrEvent.SendFollowUpEmail || meetlrEvent.FollowUpHoursAfter <= 0)
            return 0;

        var scheduledAt = DateTime.UtcNow; // Created now
        var executeAt = booking.EndTime.AddHours(meetlrEvent.FollowUpHoursAfter); // Executed later

        return await _channelDispatcher.QueueToEnabledChannelsAsync(
            booking.Id,
            meetlrEvent.Id,
            meetlrEvent.UserId,
            NotificationTrigger.BookingFollowUp,
            meetlrEvent,
            booking.Contact?.Email ?? string.Empty,
            booking.Contact?.Phone,
            scheduledAt,
            executeAt,
            cancellationToken);
    }
}
