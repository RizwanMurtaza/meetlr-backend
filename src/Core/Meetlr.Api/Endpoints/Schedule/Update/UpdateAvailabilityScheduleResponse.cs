namespace Meetlr.Api.Endpoints.Schedule.Update;

public class UpdateAvailabilityScheduleResponse
{
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string TimeZone { get; set; } = string.Empty;
    public bool IsDefault { get; set; }
    public bool Success { get; set; }
}
