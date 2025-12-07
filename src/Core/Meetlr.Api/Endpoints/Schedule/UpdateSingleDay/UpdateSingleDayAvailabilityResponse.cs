namespace Meetlr.Api.Endpoints.Schedule.UpdateSingleDay;

public class UpdateSingleDayAvailabilityResponse
{
    public Guid ScheduleId { get; set; }
    public int DayOfWeek { get; set; }
    public bool Success { get; set; }
    public string Message { get; set; } = string.Empty;
}
