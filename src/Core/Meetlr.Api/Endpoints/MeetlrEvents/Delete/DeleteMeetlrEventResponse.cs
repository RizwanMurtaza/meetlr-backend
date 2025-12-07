using Meetlr.Application.Features.MeetlrEvents.Commands.Delete;

namespace Meetlr.Api.Endpoints.MeetlrEvents.Delete;

public class DeleteMeetlrEventResponse
{
    public bool Success { get; init; }
    public string Message { get; init; } = string.Empty;

    public static DeleteMeetlrEventResponse FromCommandResponse(DeleteMeetlrEventCommandResponse commandResponse)
    {
        return new DeleteMeetlrEventResponse
        {
            Success = commandResponse.Success,
            Message = commandResponse.Message
        };
    }
}
