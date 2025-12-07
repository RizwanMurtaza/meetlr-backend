namespace Meetlr.Application.Features.MeetlrEvents.Queries.GetTypesByUsername;

public record GetMeetlrEventsByUsernameQueryResponse
{
    public List<MeetlrEventListItem> MeetlrEvents { get; init; } = new();
    public HostInfo? Host { get; init; }
}