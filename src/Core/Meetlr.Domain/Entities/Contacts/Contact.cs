using Meetlr.Domain.Common;
using Meetlr.Domain.Entities.Events;
using Meetlr.Domain.Entities.Users;
using Meetlr.Domain.Enums;

namespace Meetlr.Domain.Entities.Contacts;

/// <summary>
/// Represents a contact (invitee/customer) who books appointments
/// </summary>
public class Contact : BaseAuditableEntity
{
    // TenantId inherited from BaseAuditableEntity

    /// <summary>
    /// User who owns/created this contact
    /// </summary>
    public Guid UserId { get; set; }

    /// <summary>
    /// If true, this contact is visible across all tenants (shared resource)
    /// </summary>
    public bool IsShared { get; set; }

    // Core contact information
    public string Name { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string? Phone { get; set; }
    public string? TimeZone { get; set; }

    // Additional details
    public string? Company { get; set; }
    public string? JobTitle { get; set; }
    public string? ProfileImageUrl { get; set; }
    public string? PreferredLanguage { get; set; }

    /// <summary>
    /// JSON field for flexible custom data specific to the tenant
    /// </summary>
    public string? CustomFieldsJson { get; set; }

    /// <summary>
    /// Comma-separated tags for categorization
    /// </summary>
    public string? Tags { get; set; }

    // Tracking and management
    public ContactSource Source { get; set; } = ContactSource.Booking;
    public bool IsBlacklisted { get; set; }
    public string? BlockedReason { get; set; }
    public bool MarketingConsent { get; set; }

    /// <summary>
    /// Last time this contact was contacted (booking created, email sent, etc.)
    /// </summary>
    public DateTime? LastContactedAt { get; set; }

    /// <summary>
    /// Total number of bookings made by this contact
    /// </summary>
    public int TotalBookings { get; set; }

    /// <summary>
    /// Number of times this contact was marked as no-show
    /// </summary>
    public int NoShowCount { get; set; }

    // Navigation properties
    public User Owner { get; set; } = null!;
    public ICollection<Booking> Bookings { get; set; } = new List<Booking>();
}
