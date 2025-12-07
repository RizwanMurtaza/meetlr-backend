using Meetlr.Domain.Common;
using Meetlr.Domain.Entities.Users;
using Meetlr.Domain.Enums;

namespace Meetlr.Domain.Entities.Plugins;

/// <summary>
/// Generic entity to track which plugins (of any type) each user has installed/enabled
/// Supports: Payment providers, Calendar integrations, Communication tools, etc.
/// </summary>
public class UserInstalledPlugin : BaseAuditableEntity
{
    public Guid UserId { get; set; }
    public User User { get; set; } = null!;

    /// <summary>
    /// Plugin category (Payment, Calendar, VideoConferencing, Communication, Analytics)
    /// </summary>
    public PluginCategory PluginCategory { get; set; }
    
    /// <summary>
    /// Unique plugin identifier (e.g., "stripe", "paypal", "google-calendar", "zoom")
    /// </summary>
    public string PluginId { get; set; } = string.Empty;
    
    /// <summary>
    /// Plugin display name (e.g., "Stripe", "Google Calendar", "Zoom")
    /// </summary>
    public string PluginName { get; set; } = string.Empty;
    
    /// <summary>
    /// Plugin version installed (for future compatibility tracking)
    /// </summary>
    public string? PluginVersion { get; set; }
    
    /// <summary>
    /// Whether this plugin is currently enabled for the user
    /// </summary>
    public bool IsEnabled { get; set; } = true;
    
    /// <summary>
    /// Whether the plugin account/integration is connected and ready
    /// </summary>
    public bool IsConnected { get; set; }
    
    /// <summary>
    /// Connection/verification status (e.g., "pending", "verified", "unverified", "expired")
    /// </summary>
    public string? ConnectionStatus { get; set; }
    
    /// <summary>
    /// When the plugin was installed/added by the user
    /// </summary>
    public DateTime InstalledAt { get; set; } = DateTime.UtcNow;
    
    /// <summary>
    /// When the plugin account was successfully connected
    /// </summary>
    public DateTime? ConnectedAt { get; set; }
    
    /// <summary>
    /// When the connection/access expires (for OAuth tokens, subscriptions, etc.)
    /// </summary>
    public DateTime? ExpiresAt { get; set; }
    
    /// <summary>
    /// Configuration/settings specific to this plugin for this user (stored as JSON)
    /// Examples: API keys, preferences, feature flags, etc.
    /// </summary>
    public string? Settings { get; set; }
    
    /// <summary>
    /// Metadata about the plugin installation (stored as JSON)
    /// Examples: OAuth scopes, permissions granted, webhook IDs, etc.
    /// </summary>
    public string? Metadata { get; set; }
    
    /// <summary>
    /// Last time plugin status/data was synced
    /// </summary>
    public DateTime? LastSyncedAt { get; set; }
    
    /// <summary>
    /// Error message if plugin connection failed or has issues
    /// </summary>
    public string? ErrorMessage { get; set; }

    /// <summary>
    /// Number of times this plugin has been used (for analytics)
    /// </summary>
    public int UsageCount { get; set; }

    /// <summary>
    /// Last time this plugin was used
    /// </summary>
    public DateTime? LastUsedAt { get; set; }

    // OAuth Token Storage (for plugins that require OAuth like Google Meet, Microsoft Teams)

    /// <summary>
    /// OAuth access token for API calls
    /// </summary>
    public string? AccessToken { get; set; }

    /// <summary>
    /// OAuth refresh token for getting new access tokens
    /// </summary>
    public string? RefreshToken { get; set; }

    /// <summary>
    /// When the access token expires
    /// </summary>
    public DateTime? TokenExpiresAt { get; set; }

    /// <summary>
    /// Email/account identifier from the OAuth provider
    /// </summary>
    public string? ProviderEmail { get; set; }

    /// <summary>
    /// Provider-specific user ID
    /// </summary>
    public string? ProviderId { get; set; }
}
