namespace Meetlr.Api.Endpoints.Public.CheckSlotAvailability;

public class CheckSlotAvailabilityRequest
{
    public DateTime StartTime { get; set; }
    public int DurationMinutes { get; set; } = 30;
    public string? UserSlug { get; set; }
    public string? TimeZone { get; set; }
}
