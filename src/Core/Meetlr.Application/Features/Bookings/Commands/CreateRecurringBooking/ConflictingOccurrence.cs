namespace Meetlr.Application.Features.Bookings.Commands.CreateRecurringBooking;

public record ConflictingOccurrence
{
    public int OccurrenceNumber { get; init; }
    public DateTime RequestedDate { get; init; }
    public DateTime RequestedTime { get; init; }
    public List<AlternativeSlot> SuggestedSlots { get; init; } = new();
}
