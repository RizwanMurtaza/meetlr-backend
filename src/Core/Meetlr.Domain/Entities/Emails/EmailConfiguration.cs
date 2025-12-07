using Meetlr.Domain.Common;
using Meetlr.Domain.Entities.Tenancy;
using Meetlr.Domain.Entities.Users;

namespace Meetlr.Domain.Entities.Emails;

/// <summary>
/// SMTP configuration entity with hierarchical support (System → Tenant → User)
/// Users can have multiple configurations but only one can be active at a time
/// </summary>
public class EmailConfiguration : BaseAuditableEntity
{
    /// <summary>
    /// Tenant ID for tenant-level SMTP config (null = system config)
    /// </summary>
    public new Guid? TenantId { get; set; }

    /// <summary>
    /// User ID for user-level SMTP config (null = tenant/system config)
    /// </summary>
    public Guid? UserId { get; set; }

    /// <summary>
    /// SMTP server hostname (e.g., smtp.gmail.com)
    /// </summary>
    public string SmtpHost { get; set; } = string.Empty;

    /// <summary>
    /// SMTP server port (typically 587 for TLS, 465 for SSL, 25 for unencrypted)
    /// </summary>
    public int SmtpPort { get; set; } = 587;

    /// <summary>
    /// SMTP username for authentication
    /// </summary>
    public string SmtpUsername { get; set; } = string.Empty;

    /// <summary>
    /// SMTP password for authentication (stored in plain text as per requirement)
    /// </summary>
    public string SmtpPassword { get; set; } = string.Empty;

    /// <summary>
    /// Email address to use in the "From" field
    /// </summary>
    public string FromEmail { get; set; } = string.Empty;

    /// <summary>
    /// Display name to use in the "From" field
    /// </summary>
    public string FromName { get; set; } = string.Empty;

    /// <summary>
    /// Whether to use SSL/TLS encryption
    /// </summary>
    public bool EnableSsl { get; set; } = true;

    /// <summary>
    /// Whether this configuration is active (only one per user can be active)
    /// </summary>
    public bool IsActive { get; set; } = true;

    /// <summary>
    /// Indicates if this is the system default configuration
    /// </summary>
    public bool IsSystemDefault { get; set; } = false;

    /// <summary>
    /// Last time this configuration was tested
    /// </summary>
    public DateTime? LastTestedAt { get; set; }

    /// <summary>
    /// Result of the last test (true = success, false = failure, null = not tested)
    /// </summary>
    public bool? LastTestSucceeded { get; set; }

    /// <summary>
    /// Error message from last test failure
    /// </summary>
    public string? LastTestError { get; set; }

    // Navigation properties
    public Tenant? Tenant { get; set; }
    public User? User { get; set; }
}
