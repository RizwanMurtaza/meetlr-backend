namespace Meetlr.Api.Endpoints.Bookings.CreateRecurring;

public record AlternativeSlotDto
{
    public DateTime StartTime { get; init; }
    public DateTime EndTime { get; init; }
    public string DisplayTime { get; init; } = string.Empty;
}
