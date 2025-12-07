namespace Meetlr.Module.Calendar.Application.Queries.GetCalendarBusyTimes;

public class BusyTimeSlotDto
{
    public DateTime StartTime { get; set; }
    public DateTime EndTime { get; set; }
    public string? Summary { get; set; }
    public string Provider { get; set; } = string.Empty;
}
