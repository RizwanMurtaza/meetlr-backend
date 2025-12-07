namespace Meetlr.Application.Features.Schedule.Commands.UpdateScheduleTimeZone;

public class UpdateScheduleTimeZoneResponse
{
    public Guid Id { get; set; }
    public string TimeZone { get; set; } = string.Empty;
    public bool Success { get; set; }
}