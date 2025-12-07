using Meetlr.Application.Plugins.Models;
using Meetlr.Domain.Enums;

namespace Meetlr.Application.Plugins;

/// <summary>
/// Base interface that ALL plugins must implement.
/// Provides common functionality for plugin management (identity, status, connection).
/// Category-specific operations are in derived interfaces (IPaymentPlugin, IMeetingTypesPlugin, etc.)
/// </summary>
public interface IPlugin
{
    /// <summary>
    /// Plugin category enum
    /// </summary>
    PluginCategory Category { get; }

    /// <summary>
    /// Unique identifier for this plugin (e.g., "stripe", "zoom", "google-meet")
    /// </summary>
    string PluginId { get; }

    /// <summary>
    /// Display name for the plugin
    /// </summary>
    string PluginName { get; }

    /// <summary>
    /// Plugin description
    /// </summary>
    string Description { get; }

    /// <summary>
    /// Plugin version
    /// </summary>
    string Version { get; }

    /// <summary>
    /// Plugin author/developer
    /// </summary>
    string? Author { get; }

    /// <summary>
    /// Plugin icon URL or path
    /// </summary>
    string? IconUrl { get; }

    /// <summary>
    /// Whether this plugin is currently enabled (based on configuration)
    /// </summary>
    bool IsEnabled { get; }

    /// <summary>
    /// Whether this plugin requires user authentication/connection (OAuth)
    /// </summary>
    bool RequiresConnection { get; }

    /// <summary>
    /// Get plugin health status
    /// </summary>
    Task<PluginHealthStatus> GetHealthStatusAsync(CancellationToken cancellationToken = default);

    /// <summary>
    /// Get connection status for a specific user
    /// </summary>
    Task<PluginConnectionStatus?> GetConnectionStatusAsync(Guid userId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Generate OAuth/Connect URL for account linking.
    /// Returns null if plugin doesn't require connection.
    /// </summary>
    Task<string?> GenerateConnectUrlAsync(Guid userId, string returnUrl, CancellationToken cancellationToken = default);

    /// <summary>
    /// Complete the OAuth/Connect flow with the authorization code
    /// </summary>
    Task<bool> CompleteConnectAsync(string authorizationCode, string redirectUri, Guid userId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Disconnect/Revoke the connected account for a user
    /// </summary>
    Task<bool> DisconnectAsync(Guid userId, CancellationToken cancellationToken = default);
}

