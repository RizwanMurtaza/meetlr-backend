namespace Meetlr.Module.Calendar.Application.Commands.RescheduleCalendarEventForBooking;

public record RescheduleCalendarEventForBookingCommandResponse
{
    public bool Success { get; init; }
    public string? NewCalendarEventId { get; init; }
    public string? ErrorMessage { get; init; }
}
