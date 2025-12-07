namespace Meetlr.Plugins.MeetingTypes.Providers.Jitsi;

/// <summary>
/// Configuration settings for Jitsi Meet
/// </summary>
public class JitsiSettings
{
    /// <summary>
    /// Jitsi server URL (default: https://meet.jit.si)
    /// </summary>
    public string ServerUrl { get; set; } = "https://meet.jit.si";

    /// <summary>
    /// Room name prefix
    /// </summary>
    public string RoomPrefix { get; set; } = "meetlr";
}
