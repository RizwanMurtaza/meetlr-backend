using MediatR;
using Meetlr.Application.Features.Notifications.Models;
using Meetlr.Domain.Entities.Notifications;
using Meetlr.Module.Calendar.Application.Commands.RescheduleCalendarEventForBooking;

namespace Meetlr.Scheduler.Services.Notifications.Handlers;

public class CalendarRescheduleHandler : INotificationTypeHandler
{
    public async Task<NotificationHandlerResult> HandleAsync(
        NotificationPending notification,
        NotificationPayload? payload,
        IMediator mediator,
        CancellationToken cancellationToken)
    {
        var command = new RescheduleCalendarEventForBookingCommand
        {
            NotificationPendingId = notification.Id
        };

        var result = await mediator.Send(command, cancellationToken);

        return new NotificationHandlerResult
        {
            Success = result.Success,
            MessageId = result.NewCalendarEventId,
            ErrorMessage = result.ErrorMessage
        };
    }
}
