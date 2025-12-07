namespace Meetlr.Application.Features.Schedule.Commands.UpdateAvailabilitySchedule;

public class UpdateAvailabilityScheduleCommandResponse
{
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string TimeZone { get; set; } = string.Empty;
    public bool IsDefault { get; set; }
    public bool Success { get; set; }
}
