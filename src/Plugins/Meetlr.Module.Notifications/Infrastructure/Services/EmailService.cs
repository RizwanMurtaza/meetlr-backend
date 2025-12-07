using Meetlr.Application.Interfaces;
using Microsoft.Extensions.Logging;

namespace Meetlr.Module.Notifications.Infrastructure.Services;

/// <summary>
/// Email service with provider abstraction support
/// Supports multiple email providers (SendGrid, Mailchimp, SMTP) with priority-based failover
/// </summary>
public class EmailService : IEmailService
{
    private readonly IEnumerable<IEmailProvider> _emailProviders;
    private readonly ILogger<EmailService> _logger;

    public EmailService(
        IEnumerable<IEmailProvider> emailProviders,
        ILogger<EmailService> logger)
    {
        _emailProviders = emailProviders.OrderBy(p => p.Priority); // Order by priority (lower = higher priority)
        _logger = logger;
    }

    public async Task SendEmailAsync(string to, string subject, string body, CancellationToken cancellationToken = default)
    {
        await SendEmailWithProvidersAsync(to, subject, body, null, null, null, new List<EmailAttachment>(), cancellationToken);
    }

    public async Task SendEmailAsync(string to, string subject, string body, List<EmailAttachment> attachments, CancellationToken cancellationToken = default)
    {
        await SendEmailWithProvidersAsync(to, subject, body, null, null, null, attachments, cancellationToken);
    }

    /// <summary>
    /// Send email with provider failover (SendGrid → Mailchimp → SMTP)
    /// </summary>
    private async Task SendEmailWithProvidersAsync(
        string to,
        string subject,
        string htmlBody,
        string? plainTextBody,
        Guid? tenantId,
        Guid? userId,
        List<EmailAttachment> attachments,
        CancellationToken cancellationToken)
    {
        _logger.LogDebug("Attempting to send email to {To} with subject '{Subject}'", to, subject);

        var request = new EmailSendRequest
        {
            To = to,
            Subject = subject,
            HtmlBody = htmlBody,
            PlainTextBody = plainTextBody,
            TenantId = tenantId,
            UserId = userId,
            Attachments = attachments
        };

        Exception? lastException = null;

        // Try each provider in priority order
        foreach (var provider in _emailProviders)
        {
            try
            {
                // Check if provider is available
                if (!await provider.IsAvailableAsync(cancellationToken))
                {
                    _logger.LogDebug("Provider {ProviderName} is not available, skipping", provider.ProviderName);
                    continue;
                }

                _logger.LogDebug("Attempting to send email via {ProviderName}", provider.ProviderName);

                var result = await provider.SendEmailAsync(request, cancellationToken);

                if (result.Success)
                {
                    _logger.LogInformation(
                        "Email sent successfully to {To} via {Provider}, MessageId: {MessageId}",
                        to, result.ProviderUsed, result.MessageId);
                    return; // Success!
                }

                _logger.LogWarning(
                    "Provider {ProviderName} failed to send email: {Error}",
                    provider.ProviderName, result.ErrorMessage);
                lastException = new Exception(result.ErrorMessage);
            }
            catch (Exception ex)
            {
                lastException = ex;
                _logger.LogWarning(ex, "Exception using provider {ProviderName}", provider.ProviderName);
            }
        }

        // All providers failed
        _logger.LogError(lastException,
            "Failed to send email to {To} after trying all available providers", to);
    }
}
