namespace Meetlr.Application.Features.MeetlrEvents.Commands.SaveEmailTemplate;

public class SaveEventEmailTemplateResponse
{
    public Guid Id { get; set; }
    public bool Success { get; set; }
    public string? Message { get; set; }
}
