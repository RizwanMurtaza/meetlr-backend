using Meetlr.Domain.Common;
using Meetlr.Domain.Entities.Tenancy;
using Meetlr.Domain.Entities.Users;

namespace Meetlr.Domain.Entities.Emails;

/// <summary>
/// Email provider configuration entity (SendGrid, Mailchimp, etc.)
/// Supports hierarchical configuration (System → Tenant → User)
/// </summary>
public class EmailProviderConfiguration : BaseAuditableEntity
{
    /// <summary>
    /// Provider type (SendGrid, Mailchimp, etc.)
    /// </summary>
    public string ProviderType { get; set; } = string.Empty;

    /// <summary>
    /// Tenant ID for tenant-level config (null = system config)
    /// </summary>
    public new Guid? TenantId { get; set; }

    /// <summary>
    /// User ID for user-level config (null = tenant/system config)
    /// </summary>
    public Guid? UserId { get; set; }

    /// <summary>
    /// API Key for the provider
    /// </summary>
    public string ApiKey { get; set; } = string.Empty;

    /// <summary>
    /// Default "From" email address
    /// </summary>
    public string DefaultFromEmail { get; set; } = string.Empty;

    /// <summary>
    /// Default "From" name
    /// </summary>
    public string DefaultFromName { get; set; } = string.Empty;

    /// <summary>
    /// Whether this configuration is active
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

    /// <summary>
    /// Additional provider-specific settings in JSON format
    /// </summary>
    public string? AdditionalSettings { get; set; }

    // Navigation properties
    public Tenant? Tenant { get; set; }
    public User? User { get; set; }
}
