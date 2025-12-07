using Meetlr.Domain.Common;
using Meetlr.Module.Billing.Domain.Enums;

namespace Meetlr.Module.Billing.Domain.Entities;

/// <summary>
/// System-defined subscription packages available for purchase.
/// This is a global entity (same across all tenants).
/// </summary>
public class Package : BaseGlobalAuditableEntity
{
    /// <summary>
    /// Display name of the package (e.g., "Starter", "Pro", "Enterprise")
    /// </summary>
    public string Name { get; set; } = string.Empty;

    /// <summary>
    /// Description of what's included in the package
    /// </summary>
    public string Description { get; set; } = string.Empty;

    /// <summary>
    /// Type of package (PayAsYouGo, Monthly, Yearly, Unlimited)
    /// </summary>
    public PackageType Type { get; set; }

    /// <summary>
    /// Price in USD (or base currency)
    /// </summary>
    public decimal Price { get; set; }

    /// <summary>
    /// Number of credits included per billing period.
    /// Null indicates unlimited credits.
    /// </summary>
    public int? CreditsIncluded { get; set; }

    /// <summary>
    /// Percentage of unused credits that roll over to next period (0-100).
    /// Null means no rollover.
    /// </summary>
    public int? RolloverPercentage { get; set; }

    /// <summary>
    /// Duration of the package in days (e.g., 30 for monthly, 365 for yearly)
    /// </summary>
    public int DurationDays { get; set; }

    /// <summary>
    /// Whether this package is currently available for purchase
    /// </summary>
    public bool IsActive { get; set; } = true;

    /// <summary>
    /// Display order in the UI (lower = higher priority)
    /// </summary>
    public int SortOrder { get; set; }

    /// <summary>
    /// Stripe Price ID for subscription billing
    /// </summary>
    public string? StripePriceId { get; set; }

    // Helper Methods

    /// <summary>
    /// Check if this is an unlimited package
    /// </summary>
    public bool IsUnlimited => Type == PackageType.Unlimited || !CreditsIncluded.HasValue;
}
