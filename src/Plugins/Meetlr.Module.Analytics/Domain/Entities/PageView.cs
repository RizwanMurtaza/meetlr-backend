using Meetlr.Domain.Common;
using Meetlr.Module.Analytics.Domain.Enums;

namespace Meetlr.Module.Analytics.Domain.Entities;

/// <summary>
/// Represents a page view event for analytics tracking
/// </summary>
public class PageView : ITenantScoped
{
    /// <summary>
    /// Unique identifier for the page view
    /// </summary>
    public Guid Id { get; set; }

    /// <summary>
    /// The user whose page was viewed (event owner)
    /// </summary>
    public Guid? UserId { get; set; }

    /// <summary>
    /// The tenant ID for multi-tenant tracking
    /// </summary>
    public Guid TenantId { get; set; }

    /// <summary>
    /// The specific event that was viewed (null for homepage/event list)
    /// </summary>
    public Guid? MeetlrEventId { get; set; }

    /// <summary>
    /// Type of page that was viewed
    /// </summary>
    public PageViewType PageType { get; set; }

    /// <summary>
    /// Full path of the page that was viewed
    /// </summary>
    public string PagePath { get; set; } = string.Empty;

    /// <summary>
    /// Username from the URL (for resolving UserId)
    /// </summary>
    public string Username { get; set; } = string.Empty;

    /// <summary>
    /// Event slug from the URL (for resolving MeetlrEventId)
    /// </summary>
    public string? EventSlug { get; set; }

    /// <summary>
    /// When the page was viewed
    /// </summary>
    public DateTime ViewedAt { get; set; }

    /// <summary>
    /// Session ID for unique visitor tracking
    /// </summary>
    public string? SessionId { get; set; }

    /// <summary>
    /// Browser user agent string
    /// </summary>
    public string? UserAgent { get; set; }

    /// <summary>
    /// The referring URL
    /// </summary>
    public string? ReferrerUrl { get; set; }

    /// <summary>
    /// Device type (Desktop, Mobile, Tablet)
    /// </summary>
    public string? DeviceType { get; set; }

    /// <summary>
    /// IP address (hashed for privacy)
    /// </summary>
    public string? IpAddressHash { get; set; }

    /// <summary>
    /// Country code derived from IP
    /// </summary>
    public string? CountryCode { get; set; }

    /// <summary>
    /// When this record was created
    /// </summary>
    public DateTime CreatedAt { get; set; }
}
