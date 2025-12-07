using Meetlr.Domain.Common;
using Meetlr.Domain.Entities.Tenancy;
using Meetlr.Domain.Entities.Users;
using Meetlr.Domain.Enums;

namespace Meetlr.Domain.Entities.Emails;

/// <summary>
/// Email template entity with hierarchical support (System → Tenant → User)
/// </summary>
public class EmailTemplate : BaseAuditableEntity
{
    /// <summary>
    /// Tenant ID for tenant-level templates (null = system template)
    /// </summary>
    public new Guid? TenantId { get; set; }

    /// <summary>
    /// User ID for user-level templates (null = tenant/system template)
    /// </summary>
    public Guid? UserId { get; set; }

    /// <summary>
    /// Type of email template
    /// </summary>
    public EmailTemplateType TemplateType { get; set; }

    /// <summary>
    /// Email subject line (supports variable placeholders like {userName})
    /// </summary>
    public string Subject { get; set; } = string.Empty;

    /// <summary>
    /// HTML body of the email (supports variable placeholders)
    /// </summary>
    public string HtmlBody { get; set; } = string.Empty;

    /// <summary>
    /// Plain text version of the email (optional, for fallback)
    /// </summary>
    public string? PlainTextBody { get; set; }

    /// <summary>
    /// JSON array of available variable names for this template
    /// Example: ["userName", "bookingTime", "eventName"]
    /// </summary>
    public string AvailableVariablesJson { get; set; } = "[]";

    /// <summary>
    /// Whether this template is active and can be used
    /// </summary>
    public bool IsActive { get; set; } = true;

    /// <summary>
    /// Indicates if this is a system default template (cannot be deleted by users)
    /// </summary>
    public bool IsSystemDefault { get; set; } = false;

    // Navigation properties
    public Tenant? Tenant { get; set; }
    public User? User { get; set; }
}
