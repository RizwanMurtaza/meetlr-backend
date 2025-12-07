using MediatR;

namespace Meetlr.Application.Features.MeetlrEvents.Queries.GetEmailTemplates;

public class GetEventEmailTemplatesQuery : IRequest<List<GetEventEmailTemplatesResponse>>
{
    public Guid MeetlrEventId { get; set; }
}
