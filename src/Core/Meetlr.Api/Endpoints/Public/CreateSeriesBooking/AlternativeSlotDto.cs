namespace Meetlr.Api.Endpoints.Public.CreateSeriesBooking;

public record AlternativeSlotDto
{
    public DateTime StartTime { get; init; }
    public DateTime EndTime { get; init; }
    public string DisplayTime { get; init; } = string.Empty;
}
