namespace Meetlr.Module.Calendar.Application.Queries.GetCalendarBusyTimes;

public class GetCalendarBusyTimesResponse
{
    public List<BusyTimeSlotDto> BusyTimes { get; set; } = new();
}
