namespace Meetlr.Application.Features.Schedule.Commands.AddDateOverride;

public class AddDateOverrideCommandResponse
{
    public Guid Id { get; set; }
    public DateTime Date { get; set; }
    public bool IsAvailable { get; set; }
    public bool Success { get; set; }
}
