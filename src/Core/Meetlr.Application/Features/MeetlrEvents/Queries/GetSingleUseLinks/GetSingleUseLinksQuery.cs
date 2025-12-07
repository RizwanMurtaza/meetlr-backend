using MediatR;

namespace Meetlr.Application.Features.MeetlrEvents.Queries.GetSingleUseLinks;

public class GetSingleUseLinksQuery : IRequest<GetSingleUseLinksQueryResponse>
{
    public Guid MeetlrEventId { get; set; }
    public bool? IncludeUsed { get; set; } = false; // By default, only return unused links
}
