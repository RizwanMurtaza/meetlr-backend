using MediatR;

namespace Meetlr.Application.Features.MeetlrEvents.Commands.SyncQuestions;

public class SyncEventQuestionsCommand : IRequest<SyncEventQuestionsResponse>
{
    public Guid MeetlrEventId { get; set; }
    public List<EventQuestionItem> Questions { get; set; } = new();
}

public class EventQuestionItem
{
    public Guid? Id { get; set; } // Null for new questions
    public string QuestionText { get; set; } = string.Empty;
    public string Type { get; set; } = "Text"; // Text, TextArea, Email, Phone, SingleChoice, MultipleChoice
    public bool IsRequired { get; set; }
    public int DisplayOrder { get; set; }
    public string? Options { get; set; } // JSON array for choice questions
}
