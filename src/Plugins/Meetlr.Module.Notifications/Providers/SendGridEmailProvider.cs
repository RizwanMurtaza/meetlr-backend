using Meetlr.Application.Interfaces;
using Meetlr.Module.Notifications.Infrastructure.Services;
using Microsoft.Extensions.Logging;
using SendGrid;
using SendGrid.Helpers.Mail;

namespace Meetlr.Module.Notifications.Providers;

/// <summary>
/// SendGrid email provider implementation with database configuration
/// </summary>
public class SendGridEmailProvider : IEmailProvider
{
    private readonly IEmailProviderConfigurationResolver _configResolver;
    private readonly ILogger<SendGridEmailProvider> _logger;

    public string ProviderName => "SendGrid";
    public int Priority => 10; // Higher priority than SMTP

    public SendGridEmailProvider(
        IEmailProviderConfigurationResolver configResolver,
        ILogger<SendGridEmailProvider> logger)
    {
        _configResolver = configResolver;
        _logger = logger;
    }

    public async Task<bool> IsAvailableAsync(CancellationToken cancellationToken = default)
    {
        var config = await _configResolver.GetActiveConfigurationAsync(
            ProviderName,
            cancellationToken: cancellationToken);

        var isAvailable = config != null && !string.IsNullOrEmpty(config.ApiKey);

        _logger.LogDebug("SendGrid availability: {IsAvailable}", isAvailable);

        return isAvailable;
    }

    public async Task<EmailSendResult> SendEmailAsync(
        EmailSendRequest request,
        CancellationToken cancellationToken = default)
    {
        // Get configuration from database
        var config = await _configResolver.GetActiveConfigurationAsync(
            ProviderName,
            request.TenantId,
            request.UserId,
            cancellationToken);

        if (config == null || string.IsNullOrEmpty(config.ApiKey))
        {
            _logger.LogWarning("No SendGrid configuration found for TenantId: {TenantId}, UserId: {UserId}",
                request.TenantId, request.UserId);
            return EmailSendResult.Failed("SendGrid configuration not found", ProviderName);
        }

        try
        {
            // Create client with API key from database
            var client = new SendGridClient(config.ApiKey);

            var from = new EmailAddress(
                request.FromEmail ?? config.DefaultFromEmail,
                request.FromName ?? config.DefaultFromName);

            var to = new EmailAddress(request.To);

            var msg = MailHelper.CreateSingleEmail(
                from,
                to,
                request.Subject,
                request.PlainTextBody ?? StripHtml(request.HtmlBody),
                request.HtmlBody);

            // Add attachments (e.g., ICS calendar invites)
            if (request.Attachments.Count > 0)
            {
                foreach (var attachment in request.Attachments)
                {
                    var base64Content = Convert.ToBase64String(attachment.Content);
                    msg.AddAttachment(attachment.FileName, base64Content, attachment.ContentType);

                    _logger.LogDebug("Added SendGrid attachment: {FileName} ({ContentType}, {Size} bytes)",
                        attachment.FileName, attachment.ContentType, attachment.Content.Length);
                }
            }

            var response = await client.SendEmailAsync(msg, cancellationToken);

            if (response.IsSuccessStatusCode)
            {
                var messageId = response.Headers.GetValues("X-Message-Id")?.FirstOrDefault() ??
                               Guid.NewGuid().ToString();

                _logger.LogInformation(
                    "Email sent successfully via SendGrid to {To}, MessageId: {MessageId}",
                    request.To, messageId);

                return EmailSendResult.Successful(messageId, ProviderName);
            }
            else
            {
                var body = await response.Body.ReadAsStringAsync(cancellationToken);
                _logger.LogError(
                    "SendGrid failed to send email to {To}: {StatusCode} - {Body}",
                    request.To, response.StatusCode, body);

                return EmailSendResult.Failed(
                    $"SendGrid API error: {response.StatusCode} - {body}",
                    ProviderName);
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Exception sending email via SendGrid to {To}", request.To);
            return EmailSendResult.Failed(ex.Message, ProviderName);
        }
    }

    private static string StripHtml(string html)
    {
        if (string.IsNullOrEmpty(html)) return string.Empty;

        // Simple HTML stripping - in production, use a library like HtmlAgilityPack
        var result = System.Text.RegularExpressions.Regex.Replace(html, "<.*?>", string.Empty);
        return System.Net.WebUtility.HtmlDecode(result);
    }
}
