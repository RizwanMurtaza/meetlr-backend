using Meetlr.Application.Features.MeetlrEvents.Commands.Deactivate;

namespace Meetlr.Api.Endpoints.MeetlrEvents.Deactivate;

public class UpdateMeetlrEventStatusResponse
{
    public bool Success { get; set; }
    public bool IsActive { get; set; }
    public int CancelledBookingsCount { get; set; }
    public string? Message { get; set; }

    public static UpdateMeetlrEventStatusResponse FromCommandResponse(ActivateDeactivateMeetlrEventCommandResponse commandResponse)
    {
        return new UpdateMeetlrEventStatusResponse
        {
            Success = commandResponse.Success,
            IsActive = commandResponse.IsActive,
            CancelledBookingsCount = commandResponse.CancelledBookingsCount,
            Message = commandResponse.Message
        };
    }
}
