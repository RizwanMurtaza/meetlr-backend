using Meetlr.Application.Interfaces;

namespace Meetlr.Module.Notifications.Infrastructure.Services;

/// <summary>
/// Abstraction for email delivery providers (SMTP, SendGrid, Mailchimp, etc.)
/// </summary>
public interface IEmailProvider
{
    /// <summary>
    /// Name of the provider (e.g., "SMTP", "SendGrid", "Mailchimp")
    /// </summary>
    string ProviderName { get; }

    /// <summary>
    /// Priority for provider selection (lower = higher priority)
    /// SMTP = 100 (fallback), SendGrid = 10, Mailchimp = 20
    /// </summary>
    int Priority { get; }

    /// <summary>
    /// Check if provider is configured and available
    /// </summary>
    Task<bool> IsAvailableAsync(CancellationToken cancellationToken = default);

    /// <summary>
    /// Send an email through this provider
    /// </summary>
    /// <param name="request">Email send request</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Result with success status and message ID if successful</returns>
    Task<EmailSendResult> SendEmailAsync(EmailSendRequest request, CancellationToken cancellationToken = default);
}