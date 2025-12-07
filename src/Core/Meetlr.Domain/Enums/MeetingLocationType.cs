namespace Meetlr.Domain.Enums;

/// <summary>
/// Defines the type of location for a meeting
/// </summary>
public enum MeetingLocationType
{
    /// <summary>
    /// In-Person: Physical meeting at a specific location
    /// Requires: LocationDetails to specify the physical address
    /// </summary>
    InPerson = 1,

    /// <summary>
    /// Zoom: Virtual meeting via Zoom
    /// Optional: LocationDetails can contain meeting link/ID
    /// </summary>
    Zoom = 2,

    /// <summary>
    /// Google Meet: Virtual meeting via Google Meet
    /// Optional: LocationDetails can contain meeting link
    /// </summary>
    GoogleMeet = 3,

    /// <summary>
    /// Microsoft Teams: Virtual meeting via Microsoft Teams
    /// Optional: LocationDetails can contain meeting link
    /// </summary>
    MicrosoftTeams = 4,

    /// <summary>
    /// Phone Call: Meeting conducted via phone
    /// Optional: LocationDetails can contain phone number or instructions
    /// </summary>
    PhoneCall = 5,

    /// <summary>
    /// Jitsi Meet: Free, no-account-required virtual meeting via Jitsi
    /// Meeting link is auto-generated when booking is created
    /// </summary>
    JitsiMeet = 6,

    /// <summary>
    /// Slack Huddle: Quick audio/video meetings via Slack
    /// Requires: Slack OAuth connection
    /// </summary>
    SlackHuddle = 7
}
