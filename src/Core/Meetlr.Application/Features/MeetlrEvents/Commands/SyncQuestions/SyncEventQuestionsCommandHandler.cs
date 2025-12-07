using MediatR;
using Meetlr.Application.Common.Interfaces;
using Meetlr.Application.Interfaces;
using Meetlr.Domain.Entities.Events;
using Meetlr.Domain.Enums;
using Meetlr.Domain.Exceptions.DomainErrors.ExceptionByArea;
using Microsoft.EntityFrameworkCore;

namespace Meetlr.Application.Features.MeetlrEvents.Commands.SyncQuestions;

public class SyncEventQuestionsCommandHandler : IRequestHandler<SyncEventQuestionsCommand, SyncEventQuestionsResponse>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly ICurrentUserService _currentUserService;

    public SyncEventQuestionsCommandHandler(
        IUnitOfWork unitOfWork,
        ICurrentUserService currentUserService)
    {
        _unitOfWork = unitOfWork;
        _currentUserService = currentUserService;
    }

    public async Task<SyncEventQuestionsResponse> Handle(SyncEventQuestionsCommand request, CancellationToken cancellationToken)
    {
        var userId = _currentUserService.UserId!.Value;

        // Verify the event exists and belongs to the current user
        var meetlrEvent = await _unitOfWork.Repository<MeetlrEvent>()
            .GetQueryable()
            .FirstOrDefaultAsync(e => e.Id == request.MeetlrEventId && e.UserId == userId && !e.IsDeleted, cancellationToken);

        if (meetlrEvent == null)
        {
            throw MeetlrEventErrors.MeetlrEventNotFound(request.MeetlrEventId);
        }

        // Get existing questions
        var existingQuestions = await _unitOfWork.Repository<MeetlrEventQuestion>()
            .GetQueryable()
            .Where(q => q.MeetlrEventId == request.MeetlrEventId)
            .ToListAsync(cancellationToken);

        var existingIds = existingQuestions.Select(q => q.Id).ToHashSet();
        var requestIds = request.Questions.Where(q => q.Id.HasValue).Select(q => q.Id!.Value).ToHashSet();

        // Delete questions that are not in the request
        var toDelete = existingQuestions.Where(q => !requestIds.Contains(q.Id)).ToList();
        foreach (var question in toDelete)
        {
            _unitOfWork.Repository<MeetlrEventQuestion>().Delete(question);
        }

        var resultQuestions = new List<MeetlrEventQuestion>();

        foreach (var item in request.Questions)
        {
            var questionType = ParseQuestionType(item.Type);

            if (item.Id.HasValue && existingIds.Contains(item.Id.Value))
            {
                // Update existing question
                var existing = existingQuestions.First(q => q.Id == item.Id.Value);
                existing.QuestionText = item.QuestionText;
                existing.Type = questionType;
                existing.IsRequired = item.IsRequired;
                existing.DisplayOrder = item.DisplayOrder;
                existing.Options = item.Options;
                existing.UpdatedAt = DateTime.UtcNow;
                _unitOfWork.Repository<MeetlrEventQuestion>().Update(existing);
                resultQuestions.Add(existing);
            }
            else
            {
                // Create new question
                var newQuestion = new MeetlrEventQuestion
                {
                    Id = Guid.NewGuid(),
                    MeetlrEventId = request.MeetlrEventId,
                    QuestionText = item.QuestionText,
                    Type = questionType,
                    IsRequired = item.IsRequired,
                    DisplayOrder = item.DisplayOrder,
                    Options = item.Options,
                    CreatedAt = DateTime.UtcNow
                };
                _unitOfWork.Repository<MeetlrEventQuestion>().Add(newQuestion);
                resultQuestions.Add(newQuestion);
            }
        }

        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return new SyncEventQuestionsResponse
        {
            MeetlrEventId = request.MeetlrEventId,
            Questions = resultQuestions
                .OrderBy(q => q.DisplayOrder)
                .Select(q => new EventQuestionResponse
                {
                    Id = q.Id,
                    QuestionText = q.QuestionText,
                    Type = q.Type.ToString(),
                    IsRequired = q.IsRequired,
                    DisplayOrder = q.DisplayOrder,
                    Options = q.Options
                })
                .ToList()
        };
    }

    private static QuestionType ParseQuestionType(string type)
    {
        return type.ToLowerInvariant() switch
        {
            "text" => QuestionType.Text,
            "textarea" => QuestionType.TextArea,
            "email" => QuestionType.Email,
            "phone" => QuestionType.Phone,
            "singlechoice" => QuestionType.SingleChoice,
            "multiplechoice" => QuestionType.MultipleChoice,
            _ => QuestionType.Text
        };
    }
}
