namespace Meetlr.Application.Features.MeetlrEvents.Commands.Deactivate;

public class ActivateDeactivateMeetlrEventCommandResponse
{
    public bool Success { get; set; }
    public bool IsActive { get; set; }
    public int CancelledBookingsCount { get; set; }
    public string? Message { get; set; }
}
