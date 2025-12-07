namespace Meetlr.Application.Features.Availability.Queries.CheckCalendarBusyTimes;

public class CheckCalendarBusyTimesQueryResponse
{
    public bool HasConflicts { get; set; }
    public int BusyTimeCount { get; set; }
    public string? ConflictReason { get; set; }
}
