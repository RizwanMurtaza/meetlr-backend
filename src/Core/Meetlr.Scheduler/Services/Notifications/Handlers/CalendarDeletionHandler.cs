using MediatR;
using Meetlr.Application.Features.Notifications.Models;
using Meetlr.Domain.Entities.Notifications;
using Meetlr.Module.Calendar.Application.Commands.DeleteCalendarEventForBooking;

namespace Meetlr.Scheduler.Services.Notifications.Handlers;

public class CalendarDeletionHandler : INotificationTypeHandler
{
    public async Task<NotificationHandlerResult> HandleAsync(
        NotificationPending notification,
        NotificationPayload? payload,
        IMediator mediator,
        CancellationToken cancellationToken)
    {
        var command = new DeleteCalendarEventForBookingCommand
        {
            NotificationPendingId = notification.Id
        };

        var result = await mediator.Send(command, cancellationToken);

        return new NotificationHandlerResult
        {
            Success = result.Success,
            MessageId = null,
            ErrorMessage = result.ErrorMessage
        };
    }
}
