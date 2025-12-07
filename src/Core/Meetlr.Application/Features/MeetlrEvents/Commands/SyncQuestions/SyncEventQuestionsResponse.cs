namespace Meetlr.Application.Features.MeetlrEvents.Commands.SyncQuestions;

public class SyncEventQuestionsResponse
{
    public Guid MeetlrEventId { get; set; }
    public List<EventQuestionResponse> Questions { get; set; } = new();
}

public class EventQuestionResponse
{
    public Guid Id { get; set; }
    public string QuestionText { get; set; } = string.Empty;
    public string Type { get; set; } = string.Empty;
    public bool IsRequired { get; set; }
    public int DisplayOrder { get; set; }
    public string? Options { get; set; }
}
