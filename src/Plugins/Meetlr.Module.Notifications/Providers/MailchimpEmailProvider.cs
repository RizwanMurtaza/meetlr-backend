using System.Net.Http.Json;
using System.Text.Json;
using Meetlr.Application.Interfaces;
using Meetlr.Module.Notifications.Infrastructure.Services;
using Microsoft.Extensions.Logging;

namespace Meetlr.Module.Notifications.Providers;

/// <summary>
/// Mailchimp (Mandrill) email provider implementation with database configuration
/// Uses Mandrill Transactional Email API
/// </summary>
public class MailchimpEmailProvider : IEmailProvider
{
    private readonly IEmailProviderConfigurationResolver _configResolver;
    private readonly ILogger<MailchimpEmailProvider> _logger;
    private readonly HttpClient _httpClient;
    private const string MandrillApiUrl = "https://mandrillapp.com/api/1.0/messages/send.json";

    public string ProviderName => "Mailchimp";
    public int Priority => 20; // Lower priority than SendGrid, higher than SMTP

    public MailchimpEmailProvider(
        IEmailProviderConfigurationResolver configResolver,
        ILogger<MailchimpEmailProvider> logger,
        IHttpClientFactory httpClientFactory)
    {
        _configResolver = configResolver;
        _logger = logger;
        _httpClient = httpClientFactory.CreateClient("Mailchimp");
    }

    public async Task<bool> IsAvailableAsync(CancellationToken cancellationToken = default)
    {
        var config = await _configResolver.GetActiveConfigurationAsync(
            ProviderName,
            cancellationToken: cancellationToken);

        var isAvailable = config != null && !string.IsNullOrEmpty(config.ApiKey);

        _logger.LogDebug("Mailchimp availability: {IsAvailable}", isAvailable);

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
            _logger.LogWarning("No Mailchimp configuration found for TenantId: {TenantId}, UserId: {UserId}",
                request.TenantId, request.UserId);
            return EmailSendResult.Failed("Mailchimp configuration not found", ProviderName);
        }

        try
        {
            var mandrillRequest = new
            {
                key = config.ApiKey,
                message = new
                {
                    html = request.HtmlBody,
                    text = request.PlainTextBody,
                    subject = request.Subject,
                    from_email = request.FromEmail ?? config.DefaultFromEmail,
                    from_name = request.FromName ?? config.DefaultFromName,
                    to = new[]
                    {
                        new { email = request.To, type = "to" }
                    }
                }
            };

            var response = await _httpClient.PostAsJsonAsync(
                MandrillApiUrl,
                mandrillRequest,
                cancellationToken);

            if (response.IsSuccessStatusCode)
            {
                var responseContent = await response.Content.ReadAsStringAsync(cancellationToken);
                var result = JsonSerializer.Deserialize<MandrillResponse[]>(responseContent);

                if (result != null && result.Length > 0)
                {
                    var firstResult = result[0];

                    if (firstResult.status == "sent" || firstResult.status == "queued")
                    {
                        _logger.LogInformation(
                            "Email sent successfully via Mailchimp/Mandrill to {To}, MessageId: {MessageId}, Status: {Status}",
                            request.To, firstResult._id, firstResult.status);

                        return EmailSendResult.Successful(firstResult._id, ProviderName);
                    }
                    else
                    {
                        _logger.LogError(
                            "Mailchimp/Mandrill rejected email to {To}: Status={Status}, Reject={Reject}",
                            request.To, firstResult.status, firstResult.reject_reason);

                        return EmailSendResult.Failed(
                            $"Email rejected: {firstResult.reject_reason ?? firstResult.status}",
                            ProviderName);
                    }
                }
            }

            var errorBody = await response.Content.ReadAsStringAsync(cancellationToken);
            _logger.LogError(
                "Mailchimp/Mandrill API error for {To}: {StatusCode} - {Body}",
                request.To, response.StatusCode, errorBody);

            return EmailSendResult.Failed(
                $"Mailchimp API error: {response.StatusCode}",
                ProviderName);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Exception sending email via Mailchimp/Mandrill to {To}", request.To);
            return EmailSendResult.Failed(ex.Message, ProviderName);
        }
    }

    private record MandrillResponse(
        string email,
        string status,
        string? reject_reason,
        string _id
    );
}
