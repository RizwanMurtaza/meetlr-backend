namespace Meetlr.Application.Features.Bookings.Commands.CreateVideoMeeting;

public class CreateVideoMeetingCommandResponse
{
    public bool Success { get; set; }
    public string? MeetingLink { get; set; }
    public string? MeetingId { get; set; }
    public string? ErrorMessage { get; set; }
}
