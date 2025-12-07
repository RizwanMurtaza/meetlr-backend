using MailKit.Net.Smtp;
using MailKit.Security;
using Meetlr.Application.Common.Interfaces;
using Meetlr.Application.Interfaces;
using Meetlr.Domain.Entities.Emails;
using Meetlr.Module.Notifications.Infrastructure.Services;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using MimeKit;

namespace Meetlr.Module.Notifications.Providers;

/// <summary>
/// SMTP email provider using MailKit - supports Oracle Email Delivery Service, Gmail, and other SMTP providers
/// MailKit is required for proper STARTTLS support with Oracle Cloud Email Delivery
/// </summary>
public class SmtpEmailProvider : IEmailProvider
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly ISmtpConfigurationResolver _smtpResolver;
    private readonly ILogger<SmtpEmailProvider> _logger;

    public string ProviderName => "SMTP";
    public int Priority => 100; // Fallback provider (lowest priority)

    public SmtpEmailProvider(
        IUnitOfWork unitOfWork,
        ISmtpConfigurationResolver smtpResolver,
        ILogger<SmtpEmailProvider> logger)
    {
        _unitOfWork = unitOfWork;
        _smtpResolver = smtpResolver;
        _logger = logger;
    }

    public async Task<bool> IsAvailableAsync(CancellationToken cancellationToken = default)
    {
        // SMTP is available if we have at least one configuration
        var configs = await _smtpResolver.GetSmtpHierarchyAsync(null, null, cancellationToken);
        return configs.Any();
    }

    public async Task<EmailSendResult> SendEmailAsync(
        EmailSendRequest request,
        CancellationToken cancellationToken = default)
    {
        // Get SMTP hierarchy: User → Tenant → System
        var smtpHierarchy = await _smtpResolver.GetSmtpHierarchyAsync(
            request.TenantId,
            request.UserId,
            cancellationToken);

        if (!smtpHierarchy.Any())
        {
            _logger.LogError(
                "No SMTP configuration available for TenantId: {TenantId}, UserId: {UserId}",
                request.TenantId, request.UserId);

            return EmailSendResult.Failed("No SMTP configuration available", ProviderName);
        }

        Exception? lastException = null;

        // Try each SMTP config in hierarchy
        foreach (var smtpConfig in smtpHierarchy)
        {
            var configLevel = smtpConfig.IsSystemDefault ? "System" :
                             smtpConfig.UserId.HasValue ? "User" : "Tenant";

            _logger.LogDebug("Trying {ConfigLevel}-level SMTP: {FromEmail}", configLevel, smtpConfig.FromEmail);

            try
            {
                await SendEmailViaMailKitAsync(request, smtpConfig, cancellationToken);

                // Success! Update test status
                await UpdateSmtpTestStatusAsync(smtpConfig.Id, true, null, cancellationToken);

                _logger.LogInformation(
                    "Email sent successfully to {To} using {ConfigLevel}-level SMTP",
                    request.To, configLevel);

                return EmailSendResult.Successful(Guid.NewGuid().ToString(), $"{ProviderName} ({configLevel})");
            }
            catch (Exception ex)
            {
                lastException = ex;
                _logger.LogWarning(ex,
                    "Failed to send email using {ConfigLevel}-level SMTP: {Error}",
                    configLevel, ex.Message);

                await UpdateSmtpTestStatusAsync(smtpConfig.Id, false, ex.Message, cancellationToken);

                // Don't retry on authentication failures
                if (IsAuthenticationError(ex))
                {
                    _logger.LogError("Authentication failed for {ConfigLevel}-level SMTP", configLevel);
                    continue; // Try next config
                }
            }
        }

        // All configs failed
        _logger.LogError(lastException,
            "Failed to send email to {To} after trying all SMTP configurations in hierarchy", request.To);

        return EmailSendResult.Failed(
            lastException?.Message ?? "All SMTP configurations failed",
            ProviderName);
    }

    private async Task SendEmailViaMailKitAsync(
        EmailSendRequest request,
        EmailConfiguration smtpConfig,
        CancellationToken cancellationToken)
    {
        // Build the MIME message
        var message = new MimeMessage();
        message.From.Add(new MailboxAddress(
            request.FromName ?? smtpConfig.FromName,
            request.FromEmail ?? smtpConfig.FromEmail));
        message.To.Add(MailboxAddress.Parse(request.To));
        message.Subject = request.Subject;

        // Build the message body
        var builder = new BodyBuilder();

        // Set HTML body
        builder.HtmlBody = request.HtmlBody;

        // Set plain text body if provided
        if (!string.IsNullOrEmpty(request.PlainTextBody))
        {
            builder.TextBody = request.PlainTextBody;
        }

        // Add attachments (e.g., ICS calendar invites)
        foreach (var attachment in request.Attachments)
        {
            var contentType = ContentType.Parse(attachment.ContentType);
            builder.Attachments.Add(attachment.FileName, attachment.Content, contentType);
        }

        message.Body = builder.ToMessageBody();

        // Send using MailKit SmtpClient
        using var client = new SmtpClient();

        // Determine the secure socket options based on port and SSL setting
        SecureSocketOptions secureSocketOptions;
        if (smtpConfig.SmtpPort == 465)
        {
            // Port 465 uses implicit SSL/TLS
            secureSocketOptions = SecureSocketOptions.SslOnConnect;
        }
        else if (smtpConfig.EnableSsl)
        {
            // Port 587 typically uses STARTTLS
            secureSocketOptions = SecureSocketOptions.StartTls;
        }
        else
        {
            // No encryption
            secureSocketOptions = SecureSocketOptions.None;
        }

        // Connect to the SMTP server
        await client.ConnectAsync(
            smtpConfig.SmtpHost,
            smtpConfig.SmtpPort,
            secureSocketOptions,
            cancellationToken);

        // Authenticate
        await client.AuthenticateAsync(
            smtpConfig.SmtpUsername,
            smtpConfig.SmtpPassword,
            cancellationToken);

        // Send the message
        await client.SendAsync(message, cancellationToken);

        // Disconnect
        await client.DisconnectAsync(true, cancellationToken);
    }

    private async Task UpdateSmtpTestStatusAsync(
        Guid configId,
        bool success,
        string? errorMessage,
        CancellationToken cancellationToken)
    {
        try
        {
            var config = await _unitOfWork.Repository<EmailConfiguration>()
                .GetQueryable()
                .FirstOrDefaultAsync(c => c.Id == configId, cancellationToken);

            if (config != null)
            {
                config.LastTestedAt = DateTime.UtcNow;
                config.LastTestSucceeded = success;
                config.LastTestError = errorMessage;

                await _unitOfWork.SaveChangesAsync(cancellationToken);
            }
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Failed to update SMTP test status for config {ConfigId}", configId);
        }
    }

    private static bool IsAuthenticationError(Exception ex)
    {
        var message = ex.Message.ToLowerInvariant();
        return message.Contains("authentication") ||
               message.Contains("credentials") ||
               message.Contains("username") ||
               message.Contains("password") ||
               message.Contains("535") ||
               ex is MailKit.Security.AuthenticationException;
    }
}
