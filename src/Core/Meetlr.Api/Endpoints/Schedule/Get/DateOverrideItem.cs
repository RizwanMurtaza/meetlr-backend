namespace Meetlr.Api.Endpoints.Schedule.Get;

public class DateOverrideItem
{
    public Guid Id { get; set; }
    public DateTime Date { get; set; }
    public bool IsAvailable { get; set; }
    public TimeSpan? StartTime { get; set; }
    public TimeSpan? EndTime { get; set; }
}
