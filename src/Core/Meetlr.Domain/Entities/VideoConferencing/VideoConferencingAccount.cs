using Meetlr.Domain.Common;
using Meetlr.Domain.Entities.Users;

namespace Meetlr.Domain.Entities.VideoConferencing;

/// <summary>
/// Stores OAuth tokens for video conferencing providers that require separate authentication (e.g., Zoom).
/// Google Meet and Microsoft Teams reuse the existing CalendarIntegration tokens.
/// </summary>
public class VideoConferencingAccount : BaseAuditableEntity
{
    public Guid UserId { get; set; }

    /// <summary>
    /// Provider identifier (e.g., "zoom")
    /// </summary>
    public string Provider { get; set; } = string.Empty;

    /// <summary>
    /// Provider's unique account ID
    /// </summary>
    public string ProviderAccountId { get; set; } = string.Empty;

    /// <summary>
    /// OAuth access token
    /// </summary>
    public string? AccessToken { get; set; }

    /// <summary>
    /// OAuth refresh token
    /// </summary>
    public string? RefreshToken { get; set; }

    /// <summary>
    /// Token expiration time
    /// </summary>
    public DateTime? TokenExpiresAt { get; set; }

    /// <summary>
    /// Email associated with the account
    /// </summary>
    public string? Email { get; set; }

    /// <summary>
    /// When the account was connected
    /// </summary>
    public DateTime? ConnectedAt { get; set; }

    // Navigation properties
    public User User { get; set; } = null!;
}
