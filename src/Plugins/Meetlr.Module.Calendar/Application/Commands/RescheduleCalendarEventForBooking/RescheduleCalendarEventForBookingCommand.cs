using MediatR;

namespace Meetlr.Module.Calendar.Application.Commands.RescheduleCalendarEventForBooking;

/// <summary>
/// Command to reschedule a calendar event for a booking (processed by background service).
/// Deletes the old calendar event and creates a new one with updated times.
/// Takes NotificationPendingId to enable retry logic.
/// </summary>
public record RescheduleCalendarEventForBookingCommand : IRequest<RescheduleCalendarEventForBookingCommandResponse>
{
    public Guid NotificationPendingId { get; init; }
}
