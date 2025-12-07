namespace Meetlr.Api.Endpoints.Public.CreateSeriesBooking;

public record ConflictingOccurrenceDto
{
    public int OccurrenceNumber { get; init; }
    public DateTime RequestedDate { get; init; }
    public DateTime RequestedTime { get; init; }
    public List<AlternativeSlotDto> SuggestedSlots { get; init; } = new();
}
