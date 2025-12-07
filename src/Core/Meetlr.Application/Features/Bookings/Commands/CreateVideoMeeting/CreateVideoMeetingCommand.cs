using MediatR;

namespace Meetlr.Application.Features.Bookings.Commands.CreateVideoMeeting;

public class CreateVideoMeetingCommand : IRequest<CreateVideoMeetingCommandResponse>
{
    public Guid NotificationPendingId { get; set; }
}
