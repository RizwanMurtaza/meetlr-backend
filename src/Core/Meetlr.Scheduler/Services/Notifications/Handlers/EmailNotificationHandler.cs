using MediatR;
using Meetlr.Module.Notifications.Application.Commands.SendEmailNotification;
using Meetlr.Application.Features.Notifications.Models;
using Meetlr.Domain.Entities.Notifications;

namespace Meetlr.Scheduler.Services.Notifications.Handlers;

public class EmailNotificationHandler : INotificationTypeHandler
{
    public async Task<NotificationHandlerResult> HandleAsync(
        NotificationPending notification,
        NotificationPayload? payload,
        IMediator mediator,
        CancellationToken cancellationToken)
    {
        var command = new SendEmailNotificationCommand
        {
            NotificationPendingId = notification.Id,
            ToEmail = notification.Recipient,
            Subject = payload?.Subject ?? "Booking Notification",
            HtmlBody = payload?.HtmlBody ?? payload?.Body ?? "",
            PlainTextBody = payload?.Body,
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
