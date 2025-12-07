namespace Meetlr.Api.Endpoints.Public.CheckSlotAvailability;

public class CheckSlotAvailabilityResponse
{
    public bool IsAvailable { get; set; }
    public DateTime StartTime { get; set; }
    public DateTime EndTime { get; set; }
    public string? TimeZone { get; set; }
}
