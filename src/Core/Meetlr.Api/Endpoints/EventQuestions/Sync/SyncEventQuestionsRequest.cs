using Meetlr.Application.Features.MeetlrEvents.Commands.SyncQuestions;

namespace Meetlr.Api.Endpoints.EventQuestions.Sync;

public class SyncEventQuestionsRequest
{
    public List<QuestionItemRequest> Questions { get; set; } = new();

    public static SyncEventQuestionsCommand ToCommand(Guid eventId, SyncEventQuestionsRequest request)
    {
        return new SyncEventQuestionsCommand
        {
            MeetlrEventId = eventId,
            Questions = request.Questions.Select(q => new EventQuestionItem
            {
                Id = q.Id,
                QuestionText = q.QuestionText,
                Type = q.Type,
                IsRequired = q.IsRequired,
                DisplayOrder = q.DisplayOrder,
                Options = q.Options
            }).ToList()
        };
    }
}

public class QuestionItemRequest
{
    public Guid? Id { get; set; }
    public string QuestionText { get; set; } = string.Empty;
    public string Type { get; set; } = "Text";
    public bool IsRequired { get; set; }
    public int DisplayOrder { get; set; }
    public string? Options { get; set; }
}
