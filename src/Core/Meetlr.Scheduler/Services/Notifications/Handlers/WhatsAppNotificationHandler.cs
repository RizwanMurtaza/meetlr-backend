using MediatR;
using Meetlr.Module.Notifications.Application.Commands.SendWhatsAppNotification;
using Meetlr.Application.Features.Notifications.Models;
using Meetlr.Domain.Entities.Notifications;

namespace Meetlr.Scheduler.Services.Notifications.Handlers;

public class WhatsAppNotificationHandler : INotificationTypeHandler
{
    public async Task<NotificationHandlerResult> HandleAsync(
        NotificationPending notification,
        NotificationPayload? payload,
        IMediator mediator,
        CancellationToken cancellationToken)
    {
        var command = new SendWhatsAppNotificationCommand
        {
            NotificationPendingId = notification.Id,
            PhoneNumber = notification.Recipient,
            Message = payload?.Body ?? "",
            MediaUrl = payload?.MediaUrl,
            Metadata = payload?.Metadata
        };

        var result = await mediator.Send(command, cancellationToken);

        return new NotificationHandlerResult
        {
            Success = result.Success,
            MessageId = result.MessageId,
            ErrorMessage = result.ErrorMessage
        };
    }
}
