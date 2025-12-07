namespace Meetlr.Application.Features.Schedule.Commands.UpdateSingleDayAvailability;

public class UpdateSingleDayCommandResponse
{
    public Guid ScheduleId { get; set; }
    public int DayOfWeek { get; set; }
    public bool Success { get; set; }
    public string Message { get; set; } = string.Empty;
}
