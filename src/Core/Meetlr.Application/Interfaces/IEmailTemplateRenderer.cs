using Meetlr.Domain.Enums;

namespace Meetlr.Application.Interfaces;

/// <summary>
/// Service for rendering email templates with variable replacement
/// Supports hierarchical template resolution: User → Tenant → System
/// </summary>
public interface IEmailTemplateRenderer
{
    /// <summary>
    /// Render an email template with variable replacement
    /// </summary>
    /// <param name="templateType">Type of email template to render</param>
    /// <param name="variables">Variables to replace in the template (e.g., {userName}, {bookingTime})</param>
    /// <param name="tenantId">Tenant ID for tenant-level template lookup (optional)</param>
    /// <param name="userId">User ID for user-level template lookup (optional)</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Tuple of (subject, htmlBody, plainTextBody)</returns>
    Task<(string subject, string htmlBody, string? plainTextBody)> RenderAsync(
        EmailTemplateType templateType,
        Dictionary<string, object> variables,
        Guid? tenantId = null,
        Guid? userId = null,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Render an email template with event-specific template support
    /// Priority: Event-specific → User → Tenant → System
    /// </summary>
    /// <param name="templateType">Type of email template to render</param>
    /// <param name="variables">Variables to replace in the template</param>
    /// <param name="eventId">Event ID for event-specific template lookup</param>
    /// <param name="tenantId">Tenant ID for tenant-level template lookup (optional)</param>
    /// <param name="userId">User ID for user-level template lookup (optional)</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Tuple of (subject, htmlBody, plainTextBody)</returns>
    Task<(string subject, string htmlBody, string? plainTextBody)> RenderForEventAsync(
        EmailTemplateType templateType,
        Dictionary<string, object> variables,
        Guid eventId,
        Guid? tenantId = null,
        Guid? userId = null,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Get available variables for a specific template type
    /// </summary>
    /// <param name="templateType">Type of email template</param>
    /// <param name="tenantId">Tenant ID for tenant-level template lookup (optional)</param>
    /// <param name="userId">User ID for user-level template lookup (optional)</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Array of available variable names</returns>
    Task<string[]> GetAvailableVariablesAsync(
        EmailTemplateType templateType,
        Guid? tenantId = null,
        Guid? userId = null,
        CancellationToken cancellationToken = default);
}
