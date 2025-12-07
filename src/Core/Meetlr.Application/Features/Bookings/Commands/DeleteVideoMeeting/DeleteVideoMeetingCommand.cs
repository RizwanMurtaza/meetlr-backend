using MediatR;

namespace Meetlr.Application.Features.Bookings.Commands.DeleteVideoMeeting;

public class DeleteVideoMeetingCommand : IRequest<DeleteVideoMeetingCommandResponse>
{
    public Guid NotificationPendingId { get; set; }
}
