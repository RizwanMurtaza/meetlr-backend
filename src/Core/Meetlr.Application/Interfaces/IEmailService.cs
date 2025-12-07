namespace Meetlr.Application.Interfaces;

/// <summary>
/// Email service interface - responsible only for sending emails through providers
/// Template building and variable construction should be done in command handlers
/// </summary>
public interface IEmailService
{
    /// <summary>
    /// Send an email with the given subject and body to the specified recipient
    /// </summary>
    Task SendEmailAsync(string to, string subject, string body, CancellationToken cancellationToken = default);

    /// <summary>
    /// Send an email with the given subject, body, and attachments to the specified recipient
    /// </summary>
    Task SendEmailAsync(string to, string subject, string body, List<EmailAttachment> attachments, CancellationToken cancellationToken = default);
}
