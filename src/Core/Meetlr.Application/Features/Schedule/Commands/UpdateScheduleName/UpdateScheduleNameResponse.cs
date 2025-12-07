namespace Meetlr.Application.Features.Schedule.Commands.UpdateScheduleName;

public class UpdateScheduleNameResponse
{
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public bool Success { get; set; }
}