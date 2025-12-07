using MediatR;

namespace Meetlr.Application.Features.MeetlrEvents.Queries.GetBySlug;

public class GetMeetlrEventBySlugQuery : IRequest<GetMeetlrEventBySlugQueryResponse>
{
    public string Username { get; set; } = string.Empty;
    public string Slug { get; set; } = string.Empty;
}
