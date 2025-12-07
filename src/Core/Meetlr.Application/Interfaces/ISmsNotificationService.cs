namespace Meetlr.Application.Interfaces;

/// <summary>
/// SMS notification service interface
/// </summary>
public interface ISmsNotificationService : INotificationService
{
    Task<(bool Success, string? MessageId, string? ErrorMessage)> SendSmsAsync(
        string phoneNumber,
        string message,
        CancellationToken cancellationToken = default);
}
