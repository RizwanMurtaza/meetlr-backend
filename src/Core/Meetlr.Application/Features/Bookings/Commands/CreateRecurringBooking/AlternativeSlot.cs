namespace Meetlr.Application.Features.Bookings.Commands.CreateRecurringBooking;

public record AlternativeSlot
{
    public DateTime StartTime { get; init; }
    public DateTime EndTime { get; init; }
    public string DisplayTime { get; init; } = string.Empty;
}
