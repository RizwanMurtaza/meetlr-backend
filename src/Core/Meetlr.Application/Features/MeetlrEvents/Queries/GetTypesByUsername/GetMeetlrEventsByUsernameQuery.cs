using MediatR;

namespace Meetlr.Application.Features.MeetlrEvents.Queries.GetTypesByUsername;

public record GetMeetlrEventsByUsernameQuery : IRequest<GetMeetlrEventsByUsernameQueryResponse>
{
    public required string Username { get; init; }
}
