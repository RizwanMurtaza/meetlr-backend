using System.Text.Encodings.Web;
using System.Text.Json;
using Meetlr.Application.Common.Interfaces;
using Meetlr.Application.Interfaces;
using Meetlr.Domain.Entities.Emails;
using Meetlr.Domain.Enums;
using Meetlr.Domain.Exceptions.Http;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace Meetlr.Module.Notifications.Infrastructure.Services;

/// <summary>
/// Service for rendering email templates with variable replacement
/// Implements hierarchical template resolution: User → Tenant → System
/// </summary>
public class EmailTemplateRenderer : IEmailTemplateRenderer
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly ILogger<EmailTemplateRenderer> _logger;

    public EmailTemplateRenderer(
        IUnitOfWork unitOfWork,
        ILogger<EmailTemplateRenderer> logger)
    {
        _unitOfWork = unitOfWork;
        _logger = logger;
    }

    public async Task<(string subject, string htmlBody, string? plainTextBody)> RenderAsync(
        EmailTemplateType templateType,
        Dictionary<string, object> variables,
        Guid? tenantId = null,
        Guid? userId = null,
        CancellationToken cancellationToken = default)
    {
        _logger.LogDebug("Rendering email template: {TemplateType} for TenantId: {TenantId}, UserId: {UserId}",
            templateType, tenantId, userId);

        // Get template with hierarchy: User → Tenant → System
        var template = await GetTemplateWithHierarchyAsync(templateType, tenantId, userId, cancellationToken);

        if (template == null)
        {
            throw NotFoundException.ForEntity("EmailTemplate", templateType);
        }

        // Replace variables in subject, HTML, and plain text
        var subject = ReplaceVariables(template.Subject, variables);
        var htmlBody = ReplaceVariables(template.HtmlBody, variables);
        var plainTextBody = string.IsNullOrEmpty(template.PlainTextBody)
            ? null
            : ReplaceVariables(template.PlainTextBody, variables);

        _logger.LogDebug("Successfully rendered email template: {TemplateType}", templateType);

        return (subject, htmlBody, plainTextBody);
    }

    public async Task<(string subject, string htmlBody, string? plainTextBody)> RenderForEventAsync(
        EmailTemplateType templateType,
        Dictionary<string, object> variables,
        Guid eventId,
        Guid? tenantId = null,
        Guid? userId = null,
        CancellationToken cancellationToken = default)
    {
        _logger.LogDebug("Rendering email template for event: {TemplateType}, EventId: {EventId}",
            templateType, eventId);

        // Try event-specific template first (highest priority)
        var eventTemplate = await _unitOfWork.Repository<EventEmailTemplate>()
            .GetQueryable()
            .Where(t =>
                t.MeetlrEventId == eventId &&
                t.TemplateType == templateType &&
                t.IsActive &&
                !t.IsDeleted)
            .FirstOrDefaultAsync(cancellationToken);

        if (eventTemplate != null)
        {
            _logger.LogDebug("Found event-specific template for {TemplateType}", templateType);

            var subject = ReplaceVariables(eventTemplate.Subject, variables);
            var htmlContent = ReplaceVariables(eventTemplate.HtmlBody, variables);

            // Check if the template is already a complete HTML document
            // If so, use it directly; otherwise wrap it in the styled email template
            var htmlBody = IsCompleteHtmlDocument(htmlContent)
                ? htmlContent
                : WrapInEmailTemplate(htmlContent, templateType, variables);

            var plainTextBody = string.IsNullOrEmpty(eventTemplate.PlainTextBody)
                ? null
                : ReplaceVariables(eventTemplate.PlainTextBody, variables);

            return (subject, htmlBody, plainTextBody);
        }

        // Fall back to standard hierarchy: User → Tenant → System
        _logger.LogDebug("No event-specific template found, falling back to standard hierarchy");
        return await RenderAsync(templateType, variables, tenantId, userId, cancellationToken);
    }

    public async Task<string[]> GetAvailableVariablesAsync(
        EmailTemplateType templateType,
        Guid? tenantId = null,
        Guid? userId = null,
        CancellationToken cancellationToken = default)
    {
        var template = await GetTemplateWithHierarchyAsync(templateType, tenantId, userId, cancellationToken);

        if (template == null)
        {
            throw NotFoundException.ForEntity("EmailTemplate", templateType);
        }

        try
        {
            var variables = JsonSerializer.Deserialize<string[]>(template.AvailableVariablesJson);
            return variables ?? Array.Empty<string>();
        }
        catch (JsonException ex)
        {
            _logger.LogError(ex, "Failed to deserialize available variables for template: {TemplateType}", templateType);
            return Array.Empty<string>();
        }
    }

    /// <summary>
    /// Get template with hierarchical lookup: User → Tenant → System
    /// </summary>
    private async Task<EmailTemplate?> GetTemplateWithHierarchyAsync(
        EmailTemplateType templateType,
        Guid? tenantId,
        Guid? userId,
        CancellationToken cancellationToken)
    {
        // Try user-level template first (highest priority)
        if (userId.HasValue)
        {
            var userTemplate = await _unitOfWork.Repository<EmailTemplate>()
                .GetQueryable()
                .Where(t =>
                    t.UserId == userId &&
                    t.TemplateType == templateType &&
                    t.IsActive &&
                    !t.IsDeleted)
                .FirstOrDefaultAsync(cancellationToken);

            if (userTemplate != null)
            {
                _logger.LogDebug("Found user-level template for {TemplateType}", templateType);
                return userTemplate;
            }
        }

        // Try tenant-level template (medium priority)
        if (tenantId.HasValue)
        {
            var tenantTemplate = await _unitOfWork.Repository<EmailTemplate>()
                .GetQueryable()
                .Where(t =>
                    t.TenantId == tenantId &&
                    t.UserId == null &&
                    t.TemplateType == templateType &&
                    t.IsActive &&
                    !t.IsDeleted)
                .FirstOrDefaultAsync(cancellationToken);

            if (tenantTemplate != null)
            {
                _logger.LogDebug("Found tenant-level template for {TemplateType}", templateType);
                return tenantTemplate;
            }
        }

        // Fallback to system template (lowest priority, always available)
        var systemTemplate = await _unitOfWork.Repository<EmailTemplate>()
            .GetQueryable()
            .Where(t =>
                t.IsSystemDefault &&
                t.TemplateType == templateType &&
                !t.IsDeleted)
            .FirstOrDefaultAsync(cancellationToken);

        if (systemTemplate != null)
        {
            _logger.LogDebug("Found system-level template for {TemplateType}", templateType);
        }
        else
        {
            _logger.LogWarning("No template found for {TemplateType} at any level", templateType);
        }

        return systemTemplate;
    }

    /// <summary>
    /// Replace variables in content with actual values
    /// Supports placeholder formats: {variableName} and {{variableName}}
    /// HTML encodes values for security
    /// </summary>
    private string ReplaceVariables(string content, Dictionary<string, object> variables)
    {
        if (string.IsNullOrEmpty(content))
        {
            return content;
        }

        var result = content;

        foreach (var (key, value) in variables)
        {
            var stringValue = value?.ToString() ?? string.Empty;

            // HTML encode the value to prevent XSS attacks
            var encodedValue = HtmlEncoder.Default.Encode(stringValue);

            // Support both single curly braces {var} and double curly braces {{var}}
            var singleBracePlaceholder = $"{{{key}}}";
            var doubleBracePlaceholder = $"{{{{{key}}}}}";

            result = result.Replace(doubleBracePlaceholder, encodedValue);
            result = result.Replace(singleBracePlaceholder, encodedValue);
        }

        // Log any unreplaced variables (helpful for debugging)
        var unreplacedSingle = System.Text.RegularExpressions.Regex.Matches(result, @"(?<!\{)\{([^{}]+)\}(?!\})");
        var unreplacedDouble = System.Text.RegularExpressions.Regex.Matches(result, @"\{\{([^{}]+)\}\}");

        var allUnreplaced = unreplacedSingle.Cast<System.Text.RegularExpressions.Match>()
            .Concat(unreplacedDouble.Cast<System.Text.RegularExpressions.Match>())
            .Select(m => m.Groups[1].Value)
            .Distinct()
            .ToList();

        if (allUnreplaced.Count > 0)
        {
            _logger.LogWarning("Unreplaced variables in template: {Variables}", string.Join(", ", allUnreplaced));
        }

        return result;
    }

    /// <summary>
    /// Checks if the content is already a complete HTML document (has html/body tags)
    /// </summary>
    private static bool IsCompleteHtmlDocument(string content)
    {
        if (string.IsNullOrWhiteSpace(content))
            return false;

        var lowerContent = content.TrimStart().ToLowerInvariant();
        return lowerContent.StartsWith("<!doctype html") || lowerContent.StartsWith("<html");
    }

    /// <summary>
    /// Wraps custom event template content in the standard Meetlr email design
    /// </summary>
    private string WrapInEmailTemplate(string customContent, EmailTemplateType templateType, Dictionary<string, object> variables)
    {
        // Get color scheme based on template type
        var (headerGradientStart, headerGradientEnd, cardBgStart, cardBgEnd, cardBorder, accentColor, headerTitle) = GetTemplateColorScheme(templateType);

        // Get tenant name for branding
        var tenantName = variables.TryGetValue("tenantName", out var tn) ? tn?.ToString() ?? "Meetlr" : "Meetlr";

        return $@"<!DOCTYPE html>
<html>
<head>
  <meta charset='UTF-8'>
  <meta name='viewport' content='width=device-width, initial-scale=1.0'>
</head>
<body style='margin: 0; padding: 0; font-family: -apple-system, BlinkMacSystemFont, Segoe UI, Roboto, Helvetica, Arial, sans-serif; background-color: #f5f5f5;'>
  <table width='100%' cellpadding='0' cellspacing='0' border='0' bgcolor='#f5f5f5'>
    <tr>
      <td align='center' style='padding: 50px 20px;'>
        <table width='600' cellpadding='0' cellspacing='0' border='0' style='max-width: 600px; background-color: #ffffff; border-radius: 20px; overflow: hidden; box-shadow: 0 10px 40px rgba(0,0,0,0.08);'>
          <!-- Header -->
          <tr>
            <td align='center' bgcolor='{headerGradientStart}' style='background: linear-gradient(135deg, {headerGradientStart} 0%, {headerGradientEnd} 100%); padding: 50px 30px;'>
              <!-- Logo -->
              <table cellpadding='0' cellspacing='0' border='0' style='margin: 0 auto 25px;'>
                <tr>
                  <td align='center' bgcolor='#ffffff' style='background-color: #ffffff; width: 70px; height: 70px; border-radius: 50%; box-shadow: 0 4px 12px rgba(0,0,0,0.1);'>
                    <span style='color: {headerGradientStart}; font-size: 42px; font-weight: 900; line-height: 70px; font-family: -apple-system, BlinkMacSystemFont, Arial, sans-serif;'>M</span>
                  </td>
                </tr>
              </table>
              <h1 style='color: #ffffff; margin: 0 0 10px; font-size: 32px; font-weight: 700; letter-spacing: -0.5px;'>{HtmlEncoder.Default.Encode(tenantName)}</h1>
              <p style='color: rgba(255,255,255,0.95); margin: 0; font-size: 18px; font-weight: 500;'>{headerTitle}</p>
            </td>
          </tr>

          <!-- Content -->
          <tr>
            <td style='padding: 55px 45px;'>
              <table width='100%' cellpadding='0' cellspacing='0' border='0' style='background: linear-gradient(135deg, {cardBgStart} 0%, {cardBgEnd} 100%); border-radius: 16px; border: 1px solid {cardBorder}; margin-bottom: 35px;'>
                <tr>
                  <td style='padding: 35px 30px;'>
                    <div style='font-size: 16px; line-height: 1.6; color: #374151;'>
                      {customContent}
                    </div>
                  </td>
                </tr>
              </table>
            </td>
          </tr>

          <!-- Footer -->
          <tr>
            <td align='center' bgcolor='#f9fafb' style='background-color: #f9fafb; padding: 35px 30px; border-top: 1px solid #e5e7eb;'>
              <p style='margin: 0 0 12px; font-size: 14px; color: #6b7280;'>Powered by <strong style='color: #667eea;'>Meetlr</strong></p>
              <p style='margin: 0; font-size: 13px; color: #9ca3af;'>© {DateTime.UtcNow.Year} {HtmlEncoder.Default.Encode(tenantName)}. All rights reserved.</p>
            </td>
          </tr>
        </table>
      </td>
    </tr>
  </table>
</body>
</html>";
    }

    /// <summary>
    /// Returns color scheme based on email template type
    /// </summary>
    private static (string headerGradientStart, string headerGradientEnd, string cardBgStart, string cardBgEnd, string cardBorder, string accentColor, string headerTitle) GetTemplateColorScheme(EmailTemplateType templateType)
    {
        return templateType switch
        {
            EmailTemplateType.BookingConfirmationInvitee or
            EmailTemplateType.BookingConfirmationHost =>
                ("#667eea", "#764ba2", "#f5f3ff", "#ede9fe", "#ddd6fe", "#7c3aed", "Booking Confirmation"),

            EmailTemplateType.BookingReminder =>
                ("#f59e0b", "#d97706", "#fffbeb", "#fef3c7", "#fde68a", "#d97706", "Meeting Reminder"),

            EmailTemplateType.BookingCancellationInvitee or
            EmailTemplateType.BookingCancellationHost =>
                ("#dc2626", "#991b1b", "#fef2f2", "#fee2e2", "#fecaca", "#dc2626", "Booking Cancelled"),

            EmailTemplateType.BookingRescheduled =>
                ("#3b82f6", "#1d4ed8", "#eff6ff", "#dbeafe", "#bfdbfe", "#2563eb", "Booking Rescheduled"),

            EmailTemplateType.GeneralEmail =>
                ("#6366f1", "#4f46e5", "#eef2ff", "#e0e7ff", "#c7d2fe", "#4f46e5", "Notification"),

            _ => ("#667eea", "#764ba2", "#f5f3ff", "#ede9fe", "#ddd6fe", "#7c3aed", "Notification")
        };
    }
}
