using Meetlr.Application.Features.MeetlrEvents.Commands.Deactivate;

namespace Meetlr.Api.Endpoints.MeetlrEvents.Deactivate;

public class ActivateDeactivateMeetlrEventRequest
{
    public bool CancelFutureBookings { get; set; }
    public string? CancellationReason { get; set; }
    
    public bool IsActive { get; set; }

    public static ActivateDeactivateMeetlrEventCommand ToCommand(ActivateDeactivateMeetlrEventRequest request, Guid MeetlrEventId, Guid userId)
    {
        return new ActivateDeactivateMeetlrEventCommand
        {
            MeetlrEventId = MeetlrEventId,
            UserId = userId,
            IsActive = request.IsActive,
            CancelFutureBookings = request.CancelFutureBookings,
            CancellationReason = request.CancellationReason
        };
    }
}
