namespace Meetlr.Application.Features.MeetlrEvents.Commands.Update;

public class UpdateMeetlrEventCommandResponse
{
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Slug { get; set; } = string.Empty;
    public bool Success { get; set; }
}
