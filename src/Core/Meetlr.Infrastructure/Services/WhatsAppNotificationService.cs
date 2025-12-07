using System.Text.RegularExpressions;
using Meetlr.Application.Interfaces;
using Meetlr.Domain.Enums;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace Meetlr.Infrastructure.Services;

/// <summary>
/// WhatsApp notification service using Twilio WhatsApp API
/// Note: Requires Twilio NuGet package - Install-Package Twilio
/// </summary>
public class WhatsAppNotificationService : IWhatsAppNotificationService
{
    private readonly IConfiguration _configuration;
    private readonly ILogger<WhatsAppNotificationService> _logger;
    private readonly string _accountSid;
    private readonly string _authToken;
    private readonly string _fromPhoneNumber;

    public NotificationType NotificationType => NotificationType.WhatsApp;

    public WhatsAppNotificationService(
        IConfiguration configuration,
        ILogger<WhatsAppNotificationService> logger)
    {
        _configuration = configuration;
        _logger = logger;

        _accountSid = _configuration["Twilio:AccountSid"] ?? "";
        _authToken = _configuration["Twilio:AuthToken"] ?? "";
        _fromPhoneNumber = _configuration["Twilio:WhatsAppFromNumber"] ?? "";
    }

    public async Task<(bool Success, string? MessageId, string? ErrorMessage)> SendAsync(
        string recipient,
        string subject,
        string body,
        Dictionary<string, string>? metadata = null,
        CancellationToken cancellationToken = default)
    {
        return await SendWhatsAppAsync(recipient, body, cancellationToken: cancellationToken);
    }

    public async Task<(bool Success, string? MessageId, string? ErrorMessage)> SendWhatsAppAsync(
        string phoneNumber,
        string message,
        string? mediaUrl = null,
        CancellationToken cancellationToken = default)
    {
        try
        {
            // Validate configuration
            if (string.IsNullOrEmpty(_accountSid) || string.IsNullOrEmpty(_authToken))
            {
                _logger.LogWarning("Twilio configuration is missing. WhatsApp message will not be sent.");
                return (false, null, "Twilio configuration is missing");
            }

            // Validate phone number
            if (!await ValidateRecipientAsync(phoneNumber, cancellationToken))
            {
                return (false, null, "Invalid phone number format");
            }

            // TODO: Implement actual Twilio WhatsApp API call
            // For now, this is a stub implementation

            /*
            // Example Twilio WhatsApp implementation:
            TwilioClient.Init(_accountSid, _authToken);

            var messageResource = await MessageResource.CreateAsync(
                body: message,
                from: new PhoneNumber($"whatsapp:{_fromPhoneNumber}"),
                to: new PhoneNumber($"whatsapp:{phoneNumber}"),
                mediaUrl: !string.IsNullOrEmpty(mediaUrl) ? new List<Uri> { new Uri(mediaUrl) } : null
            );

            _logger.LogInformation("WhatsApp message sent successfully to {PhoneNumber} with SID {MessageSid}",
                phoneNumber, messageResource.Sid);

            return (true, messageResource.Sid, null);
            */

            // Stub implementation - Replace with actual Twilio integration
            _logger.LogInformation(
                "WhatsApp message would be sent to {PhoneNumber}: {Message}, Media: {MediaUrl}",
                phoneNumber, message, mediaUrl ?? "none");

            var messageId = Guid.NewGuid().ToString();

            // Simulate async operation
            await Task.Delay(100, cancellationToken);

            return (true, messageId, null);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to send WhatsApp message to {PhoneNumber}", phoneNumber);
            return (false, null, ex.Message);
        }
    }

    public async Task<(bool Success, string? MessageId, string? ErrorMessage)> SendWhatsAppTemplateAsync(
        string phoneNumber,
        string templateName,
        Dictionary<string, string> templateParameters,
        CancellationToken cancellationToken = default)
    {
        try
        {
            // Validate configuration
            if (string.IsNullOrEmpty(_accountSid) || string.IsNullOrEmpty(_authToken))
            {
                _logger.LogWarning("Twilio configuration is missing. WhatsApp template will not be sent.");
                return (false, null, "Twilio configuration is missing");
            }

            // TODO: Implement Twilio WhatsApp template API call
            // Templates are pre-approved messages that can be sent to users

            _logger.LogInformation(
                "WhatsApp template {TemplateName} would be sent to {PhoneNumber}",
                templateName, phoneNumber);

            var messageId = Guid.NewGuid().ToString();
            await Task.Delay(100, cancellationToken);

            return (true, messageId, null);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to send WhatsApp template to {PhoneNumber}", phoneNumber);
            return (false, null, ex.Message);
        }
    }

    public async Task<bool> ValidateRecipientAsync(string recipient, CancellationToken cancellationToken = default)
    {
        // Basic phone number validation (E.164 format: +1234567890)
        var phoneRegex = new Regex(@"^\+?[1-9]\d{1,14}$");
        return await Task.FromResult(phoneRegex.IsMatch(recipient));
    }
}
