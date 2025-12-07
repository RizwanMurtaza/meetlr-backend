namespace Meetlr.Api.Endpoints.EventQuestions.Sync;

public class SyncEventQuestionsResponse
{
    public Guid MeetlrEventId { get; set; }
    public List<QuestionItemResponse> Questions { get; set; } = new();

    public static SyncEventQuestionsResponse FromCommandResponse(
        Application.Features.MeetlrEvents.Commands.SyncQuestions.SyncEventQuestionsResponse response)
    {
        return new SyncEventQuestionsResponse
        {
            MeetlrEventId = response.MeetlrEventId,
            Questions = response.Questions.Select(q => new QuestionItemResponse
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

public class QuestionItemResponse
{
    public Guid Id { get; set; }
    public string QuestionText { get; set; } = string.Empty;
    public string Type { get; set; } = string.Empty;
    public bool IsRequired { get; set; }
    public int DisplayOrder { get; set; }
    public string? Options { get; set; }
}
