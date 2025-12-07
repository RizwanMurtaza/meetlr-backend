using MediatR;
using Meetlr.Application.Common.Interfaces;
using Meetlr.Domain.Entities.Events;
using Microsoft.EntityFrameworkCore;

namespace Meetlr.Application.Features.MeetlrEvents.Queries.GetQuestions;

public class GetEventQuestionsQueryHandler : IRequestHandler<GetEventQuestionsQuery, GetEventQuestionsResponse>
{
    private readonly IUnitOfWork _unitOfWork;

    public GetEventQuestionsQueryHandler(IUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork;
    }

    public async Task<GetEventQuestionsResponse> Handle(GetEventQuestionsQuery request, CancellationToken cancellationToken)
    {
        // Get questions for the event (public - no user check needed for booking pages)
        var questions = await _unitOfWork.Repository<MeetlrEventQuestion>()
            .GetQueryable()
            .Where(q => q.MeetlrEventId == request.MeetlrEventId)
            .OrderBy(q => q.DisplayOrder)
            .ToListAsync(cancellationToken);

        return new GetEventQuestionsResponse
        {
            MeetlrEventId = request.MeetlrEventId,
            Questions = questions.Select(q => new EventQuestionDto
            {
                Id = q.Id,
                QuestionText = q.QuestionText,
                Type = q.Type.ToString(),
                IsRequired = q.IsRequired,
                DisplayOrder = q.DisplayOrder,
                Options = q.Options
            }).ToList()
        };
    }
}
