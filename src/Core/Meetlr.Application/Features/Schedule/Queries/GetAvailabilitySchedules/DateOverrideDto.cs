namespace Meetlr.Application.Features.Schedule.Queries.GetAvailabilitySchedules;

public class DateOverrideDto
{
    public Guid Id { get; set; }
    public DateTime Date { get; set; }
    public bool IsAvailable { get; set; }
    public TimeSpan? StartTime { get; set; }
    public TimeSpan? EndTime { get; set; }
}
