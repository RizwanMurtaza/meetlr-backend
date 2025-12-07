using Meetlr.Application.Features.MeetlrEvents.Commands.Update;

namespace Meetlr.Api.Endpoints.MeetlrEvents.Update;

public class UpdateMeetlrEventResponse
{
    public Guid Id { get; init; }
    public string Name { get; init; } = string.Empty;
    public string Slug { get; init; } = string.Empty;
    public bool Success { get; init; }

    public static UpdateMeetlrEventResponse FromCommandResponse(UpdateMeetlrEventCommandResponse commandResponse)
    {
        return new UpdateMeetlrEventResponse
        {
            Id = commandResponse.Id,
            Name = commandResponse.Name,
            Slug = commandResponse.Slug,
            Success = commandResponse.Success
        };
    }
}
