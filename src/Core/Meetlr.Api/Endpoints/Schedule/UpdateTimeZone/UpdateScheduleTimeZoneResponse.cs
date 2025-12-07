namespace Meetlr.Api.Endpoints.Schedule.UpdateTimeZone;

public class UpdateScheduleTimeZoneResponse
{
    public Guid Id { get; set; }
    public string TimeZone { get; set; } = string.Empty;
    public bool Success { get; set; }
}
