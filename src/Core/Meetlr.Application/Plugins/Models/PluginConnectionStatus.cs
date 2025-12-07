namespace Meetlr.Application.Plugins.Models;

/// <summary>
/// Unified connection status for all plugin types
/// </summary>
public record PluginConnectionStatus
{
    /// <summary>
    /// Whether the user has connected this plugin
    /// </summary>
    public bool IsConnected { get; init; }

    /// <summary>
    /// Email associated with the connected account (if applicable)
    /// </summary>
    public string? Email { get; init; }

    /// <summary>
    /// When the account was connected
    /// </summary>
    public DateTime? ConnectedAt { get; init; }

    /// <summary>
    /// Whether the connection needs to be refreshed (token expired, etc.)
    /// </summary>
    public bool NeedsReconnect { get; init; }

    /// <summary>
    /// Category-specific metadata (e.g., payment: charges_enabled, calendar: calendar_id)
    /// </summary>
    public Dictionary<string, object>? Metadata { get; init; }
}
