using MediatR;
using Meetlr.Application.Features.Notifications.Models;
using Meetlr.Application.Features.Payments.Commands.ProcessRefund;
using Meetlr.Domain.Entities.Notifications;

namespace Meetlr.Scheduler.Services.Notifications.Handlers;

public class RefundNotificationHandler : INotificationTypeHandler
{
    public async Task<NotificationHandlerResult> HandleAsync(
        NotificationPending notification,
        NotificationPayload? payload,
        IMediator mediator,
        CancellationToken cancellationToken)
    {
        var command = new ProcessRefundCommand
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
