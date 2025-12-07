using MediatR;

namespace Meetlr.Application.Features.MeetlrEvents.Queries.GetEventTypes;

public class GetMeetlrEventsQuery : IRequest<GetMeetlrEventsQueryResponse>
{
    public Guid UserId { get; set; }
}
