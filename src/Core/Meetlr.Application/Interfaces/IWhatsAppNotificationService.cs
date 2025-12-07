namespace Meetlr.Application.Interfaces;

/// <summary>
/// WhatsApp notification service interface
/// </summary>
public interface IWhatsAppNotificationService : INotificationService
{
    Task<(bool Success, string? MessageId, string? ErrorMessage)> SendWhatsAppAsync(
        string phoneNumber,
        string message,
        string? mediaUrl = null,
        CancellationToken cancellationToken = default);

    Task<(bool Success, string? MessageId, string? ErrorMessage)> SendWhatsAppTemplateAsync(
        string phoneNumber,
        string templateName,
        Dictionary<string, string> templateParameters,
        CancellationToken cancellationToken = default);
}
