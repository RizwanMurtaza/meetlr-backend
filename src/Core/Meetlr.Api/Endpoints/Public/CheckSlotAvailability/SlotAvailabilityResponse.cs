namespace Meetlr.Api.Endpoints.Public.CheckSlotAvailability;

public class SlotAvailabilityResponse
{
    public DateTime StartTime { get; set; }
    public DateTime EndTime { get; set; }
    public bool IsAvailable { get; set; }
    public string? TimeZone { get; set; }
}
