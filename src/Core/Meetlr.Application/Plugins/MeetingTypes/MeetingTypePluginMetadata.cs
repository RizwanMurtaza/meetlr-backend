using Meetlr.Domain.Enums;

namespace Meetlr.Application.Plugins.MeetingTypes;

/// <summary>
/// Defines metadata and requirements for each meeting type
/// </summary>
public static class MeetingTypePluginMetadata
{
    /// <summary>
    /// Meeting types that are always available without requiring plugin installation
    /// </summary>
    public static readonly MeetingLocationType[] AlwaysAvailable =
    {
        MeetingLocationType.InPerson,
        MeetingLocationType.PhoneCall,
        MeetingLocationType.JitsiMeet
    };

    /// <summary>
    /// Meeting types that require plugin installation and OAuth connection
    /// </summary>
    public static readonly MeetingLocationType[] RequiresPlugin =
    {
        MeetingLocationType.Zoom,
        MeetingLocationType.GoogleMeet,
        MeetingLocationType.MicrosoftTeams,
        MeetingLocationType.SlackHuddle
    };

    /// <summary>
    /// Maps OAuth providers to their corresponding meeting type plugin IDs
    /// When a user signs in with an OAuth provider, the corresponding meeting type plugin is auto-installed
    /// </summary>
    public static readonly Dictionary<string, string> AutoInstallOnOAuth = new(StringComparer.OrdinalIgnoreCase)
    {
        { "Google", "googlemeet" },
        { "Microsoft", "microsoftteams" }
    };

    /// <summary>
    /// Maps MeetingLocationType to plugin IDs
    /// </summary>
    public static readonly Dictionary<MeetingLocationType, string> LocationTypeToPluginId = new()
    {
        { MeetingLocationType.Zoom, "zoom" },
        { MeetingLocationType.GoogleMeet, "googlemeet" },
        { MeetingLocationType.MicrosoftTeams, "microsoftteams" },
        { MeetingLocationType.SlackHuddle, "slackhuddle" }
    };

    /// <summary>
    /// Checks if a meeting location type is always available (no plugin required)
    /// </summary>
    public static bool IsAlwaysAvailable(MeetingLocationType locationType)
    {
        return AlwaysAvailable.Contains(locationType);
    }

    /// <summary>
    /// Checks if a meeting location type requires a plugin
    /// </summary>
    public static bool RequiresPluginInstallation(MeetingLocationType locationType)
    {
        return RequiresPlugin.Contains(locationType);
    }

    /// <summary>
    /// Gets the plugin ID to auto-install for an OAuth provider
    /// </summary>
    public static string? GetAutoInstallPluginId(string oauthProvider)
    {
        return AutoInstallOnOAuth.TryGetValue(oauthProvider, out var pluginId) ? pluginId : null;
    }

    /// <summary>
    /// Gets the plugin ID for a meeting location type
    /// </summary>
    public static string? GetPluginId(MeetingLocationType locationType)
    {
        return LocationTypeToPluginId.TryGetValue(locationType, out var pluginId) ? pluginId : null;
    }
}
