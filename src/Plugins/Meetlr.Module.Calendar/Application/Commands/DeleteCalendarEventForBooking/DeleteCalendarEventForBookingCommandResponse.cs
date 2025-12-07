namespace Meetlr.Module.Calendar.Application.Commands.DeleteCalendarEventForBooking;

public record DeleteCalendarEventForBookingCommandResponse
{
    public bool Success { get; init; }
    public string? ErrorMessage { get; init; }
}
