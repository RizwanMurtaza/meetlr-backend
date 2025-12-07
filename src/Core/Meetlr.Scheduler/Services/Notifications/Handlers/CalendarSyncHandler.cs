using MediatR;
using Meetlr.Application.Features.Notifications.Models;
using Meetlr.Domain.Entities.Notifications;
using Meetlr.Module.Calendar.Application.Commands.CreateCalendarEventForBooking;

namespace Meetlr.Scheduler.Services.Notifications.Handlers;

public class CalendarSyncHandler : INotificationTypeHandler
{
    public async Task<NotificationHandlerResult> HandleAsync(
        NotificationPending notification,
        NotificationPayload? payload,
        IMediator mediator,
        CancellationToken cancellationToken)
    {
        var command = new CreateCalendarEventForBookingCommand
        {
            NotificationPendingId = notification.Id
        };

        var result = await mediator.Send(command, cancellationToken);

        return new NotificationHandlerResult
        {
            Success = result.Success,
            MessageId = result.CalendarEventId,
            ErrorMessage = result.ErrorMessage
        };
    }
}
