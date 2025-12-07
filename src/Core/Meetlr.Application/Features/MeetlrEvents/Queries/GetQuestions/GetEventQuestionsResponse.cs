namespace Meetlr.Application.Features.MeetlrEvents.Queries.GetQuestions;

public class GetEventQuestionsResponse
{
    public Guid MeetlrEventId { get; set; }
    public List<EventQuestionDto> Questions { get; set; } = new();
}

public class EventQuestionDto
{
    public Guid Id { get; set; }
    public string QuestionText { get; set; } = string.Empty;
    public string Type { get; set; } = string.Empty;
    public bool IsRequired { get; set; }
    public int DisplayOrder { get; set; }
    public string? Options { get; set; }
}
