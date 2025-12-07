using MediatR;
using Meetlr.Application.Features.Notifications.Models;
using Meetlr.Domain.Entities.Notifications;

namespace Meetlr.Scheduler.Services.Notifications.Handlers;

/// <summary>
/// Interface for handling specific notification types.
/// Each handler is responsible for sending the command via MediatR
/// and returning the result for the history manager to process.
/// </summary>
public interface INotificationTypeHandler
{
    /// <summary>
    /// Processes a notification of the specific type.
    /// </summary>
    /// <param name="notification">The pending notification to process</param>
    /// <param name="payload">Optional parsed payload data</param>
    /// <param name="mediator">MediatR instance for sending commands</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Result containing success status, message ID, and any error message</returns>
    Task<NotificationHandlerResult> HandleAsync(
        NotificationPending notification,
        NotificationPayload? payload,
        IMediator mediator,
        CancellationToken cancellationToken);
}

/// <summary>
/// Result from processing a notification
/// </summary>
public class NotificationHandlerResult
{
    public bool Success { get; set; }
    public string? MessageId { get; set; }
    public string? ErrorMessage { get; set; }
}
