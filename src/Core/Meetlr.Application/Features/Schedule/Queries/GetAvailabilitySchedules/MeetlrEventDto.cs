namespace Meetlr.Application.Features.Schedule.Queries.GetAvailabilitySchedules;

public class MeetlrEventDto
{
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Slug { get; set; } = string.Empty;
}
