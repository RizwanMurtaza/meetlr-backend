using MediatR;

namespace Meetlr.Module.Calendar.Application.Commands.DeleteCalendarEventForBooking;

/// <summary>
/// Command to delete a calendar event for a booking (processed by background service).
/// Takes NotificationPendingId to enable retry logic.
/// </summary>
public record DeleteCalendarEventForBookingCommand : IRequest<DeleteCalendarEventForBookingCommandResponse>
{
    public Guid NotificationPendingId { get; init; }
}
