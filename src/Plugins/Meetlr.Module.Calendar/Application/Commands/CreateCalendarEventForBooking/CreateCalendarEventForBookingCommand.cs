using MediatR;

namespace Meetlr.Module.Calendar.Application.Commands.CreateCalendarEventForBooking;

/// <summary>
/// Command to create a calendar event for a booking (processed by background service).
/// Takes NotificationPendingId to enable retry logic.
/// </summary>
public record CreateCalendarEventForBookingCommand : IRequest<CreateCalendarEventForBookingCommandResponse>
{
    public Guid NotificationPendingId { get; init; }
}
