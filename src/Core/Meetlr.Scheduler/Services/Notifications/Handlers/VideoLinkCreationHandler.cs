using MediatR;
using Meetlr.Application.Features.Bookings.Commands.CreateVideoMeeting;
using Meetlr.Application.Features.Notifications.Models;
using Meetlr.Domain.Entities.Notifications;

namespace Meetlr.Scheduler.Services.Notifications.Handlers;

public class VideoLinkCreationHandler : INotificationTypeHandler
{
    public async Task<NotificationHandlerResult> HandleAsync(
        NotificationPending notification,
        NotificationPayload? payload,
        IMediator mediator,
        CancellationToken cancellationToken)
    {
        var command = new CreateVideoMeetingCommand
        {
            NotificationPendingId = notification.Id
        };

        var result = await mediator.Send(command, cancellationToken);

        return new NotificationHandlerResult
        {
            Success = result.Success,
            MessageId = result.MeetingId,
            ErrorMessage = result.ErrorMessage
        };
    }
}
