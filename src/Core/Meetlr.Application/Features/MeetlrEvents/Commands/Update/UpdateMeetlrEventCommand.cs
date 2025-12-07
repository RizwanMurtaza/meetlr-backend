using MediatR;

namespace Meetlr.Application.Features.MeetlrEvents.Commands.Update;

public class UpdateMeetlrEventCommand : IRequest<UpdateMeetlrEventCommandResponse>
{
    public Guid Id { get; set; }
    public Guid UserId { get; set; }

    // Only these 5 fields are updateable after event creation
    public string? Name { get; set; }
    public string? Color { get; set; }
    public bool? NotifyViaEmail { get; set; }
    public bool? NotifyViaSms { get; set; }
    public bool? NotifyViaWhatsApp { get; set; }
}
