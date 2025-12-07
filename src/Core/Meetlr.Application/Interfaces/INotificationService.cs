using Meetlr.Domain.Enums;

namespace Meetlr.Application.Interfaces;

/// <summary>
/// Base interface for all notification services
/// </summary>
public interface INotificationService
{
    NotificationType NotificationType { get; }

    Task<(bool Success, string? MessageId, string? ErrorMessage)> SendAsync(
        string recipient,
        string subject,
        string body,
        Dictionary<string, string>? metadata = null,
        CancellationToken cancellationToken = default);

    Task<bool> ValidateRecipientAsync(string recipient, CancellationToken cancellationToken = default);
}
