using System.Text.RegularExpressions;
using Meetlr.Application.Interfaces;
using Meetlr.Domain.Enums;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace Meetlr.Infrastructure.Services;

/// <summary>
/// SMS notification service using Twilio
/// Note: Requires Twilio NuGet package - Install-Package Twilio
/// </summary>
public class SmsNotificationService : ISmsNotificationService
{
    private readonly IConfiguration _configuration;
    private readonly ILogger<SmsNotificationService> _logger;
    private readonly string _accountSid;
    private readonly string _authToken;
    private readonly string _fromPhoneNumber;

    public NotificationType NotificationType => NotificationType.Sms;

    public SmsNotificationService(
        IConfiguration configuration,
        ILogger<SmsNotificationService> logger)
    {
        _configuration = configuration;
        _logger = logger;

        _accountSid = _configuration["Twilio:AccountSid"] ?? "";
        _authToken = _configuration["Twilio:AuthToken"] ?? "";
        _fromPhoneNumber = _configuration["Twilio:FromPhoneNumber"] ?? "";
    }

    public async Task<(bool Success, string? MessageId, string? ErrorMessage)> SendAsync(
        string recipient,
        string subject,
        string body,
        Dictionary<string, string>? metadata = null,
        CancellationToken cancellationToken = default)
    {
        return await SendSmsAsync(recipient, body, cancellationToken);
    }

    public async Task<(bool Success, string? MessageId, string? ErrorMessage)> SendSmsAsync(
        string phoneNumber,
        string message,
        CancellationToken cancellationToken = default)
    {
        try
        {
            // Validate configuration
            if (string.IsNullOrEmpty(_accountSid) || string.IsNullOrEmpty(_authToken))
            {
                _logger.LogWarning("Twilio configuration is missing. SMS will not be sent.");
                return (false, null, "Twilio configuration is missing");
            }

            // Validate phone number
            if (!await ValidateRecipientAsync(phoneNumber, cancellationToken))
            {
                return (false, null, "Invalid phone number format");
            }

            // TODO: Implement actual Twilio API call
            // For now, this is a stub implementation
            // You need to install Twilio NuGet package and implement the actual API call

            /*
            // Example Twilio implementation:
            TwilioClient.Init(_accountSid, _authToken);

            var messageResource = await MessageResource.CreateAsync(
                body: message,
                from: new PhoneNumber(_fromPhoneNumber),
                to: new PhoneNumber(phoneNumber)
            );

            _logger.LogInformation("SMS sent successfully to {PhoneNumber} with SID {MessageSid}",
                phoneNumber, messageResource.Sid);

            return (true, messageResource.Sid, null);
            */

            // Stub implementation - Replace with actual Twilio integration
            _logger.LogInformation("SMS would be sent to {PhoneNumber}: {Message}", phoneNumber, message);
            var messageId = Guid.NewGuid().ToString();

            // Simulate async operation
            await Task.Delay(100, cancellationToken);

            return (true, messageId, null);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to send SMS to {PhoneNumber}", phoneNumber);
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
