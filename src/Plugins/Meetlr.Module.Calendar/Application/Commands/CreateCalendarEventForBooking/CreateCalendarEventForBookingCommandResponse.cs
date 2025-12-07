namespace Meetlr.Module.Calendar.Application.Commands.CreateCalendarEventForBooking;

public record CreateCalendarEventForBookingCommandResponse
{
    public bool Success { get; init; }
    public string? CalendarEventId { get; init; }
    public string? ErrorMessage { get; init; }
}
