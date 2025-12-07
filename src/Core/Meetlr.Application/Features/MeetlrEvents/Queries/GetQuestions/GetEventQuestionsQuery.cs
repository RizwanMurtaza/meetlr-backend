using MediatR;

namespace Meetlr.Application.Features.MeetlrEvents.Queries.GetQuestions;

public class GetEventQuestionsQuery : IRequest<GetEventQuestionsResponse>
{
    public Guid MeetlrEventId { get; set; }
}
