using Meetlr.Application.Plugins;
using Meetlr.Application.Plugins.MeetingTypes;
using Meetlr.Application.Plugins.MeetingTypes.Models;
using Meetlr.Application.Plugins.Models;
using Meetlr.Domain.Enums;
using Microsoft.Extensions.Options;

namespace Meetlr.Plugins.MeetingTypes.Providers.Jitsi;

/// <summary>
/// Jitsi Meet provider - free, no OAuth required, always available
/// </summary>
public class JitsiMeetProvider : IMeetingTypesPlugin
{
    private readonly JitsiSettings _settings;

    public JitsiMeetProvider(IOptions<JitsiSettings> settings)
    {
        _settings = settings.Value;
    }

    // IPlugin properties
    public PluginCategory Category => PluginCategory.VideoConferencing;
    public string PluginId => "jitsi";
    public string PluginName => "Jitsi Meet";
    public string Description => "Create free Jitsi Meet video meetings for your bookings - no account required";
    public string Version => "1.0.0";
    public string? Author => "Meetlr Team";
    public string? IconUrl => "/images/plugins/jitsi.svg";
    public bool IsEnabled => true; // Always enabled
    public bool RequiresConnection => false; // No OAuth required

    // IMeetingTypesPlugin property
    public MeetingLocationType LocationType => MeetingLocationType.JitsiMeet;

    public Task<bool> IsAvailableForUserAsync(Guid userId, CancellationToken cancellationToken = default)
    {
        // Jitsi is always available - no OAuth required
        return Task.FromResult(true);
    }

    public Task<PluginConnectionStatus?> GetConnectionStatusAsync(Guid userId, CancellationToken cancellationToken = default)
    {
        // Jitsi doesn't require an account, always connected
        return Task.FromResult<PluginConnectionStatus?>(new PluginConnectionStatus
        {
            IsConnected = true,
            NeedsReconnect = false
        });
    }

    public Task<string?> GenerateConnectUrlAsync(Guid userId, string returnUrl, CancellationToken cancellationToken = default)
    {
        // Jitsi doesn't require OAuth connection
        return Task.FromResult<string?>(null);
    }

    public Task<bool> CompleteConnectAsync(string code, string redirectUri, Guid userId, CancellationToken cancellationToken = default)
    {
        // Jitsi doesn't require OAuth connection
        return Task.FromResult(true);
    }

    public Task<bool> DisconnectAsync(Guid userId, CancellationToken cancellationToken = default)
    {
        // Jitsi doesn't require OAuth connection
        return Task.FromResult(true);
    }

    public Task<PluginHealthStatus> GetHealthStatusAsync(CancellationToken cancellationToken = default)
    {
        return Task.FromResult(new PluginHealthStatus
        {
            IsHealthy = true,
            Status = "Healthy",
            Message = "Jitsi Meet is always available"
        });
    }

    public Task<VideoMeetingResult> CreateMeetingAsync(CreateMeetingRequest request, CancellationToken cancellationToken = default)
    {
        // Generate a unique room name
        var roomId = GenerateRoomId(request.EventSlug, request.BookingId);
        var joinUrl = $"{_settings.ServerUrl.TrimEnd('/')}/{roomId}";

        return Task.FromResult(new VideoMeetingResult
        {
            JoinUrl = joinUrl,
            MeetingId = roomId
        });
    }

    public Task<bool> DeleteMeetingAsync(string meetingId, Guid userId, CancellationToken cancellationToken = default)
    {
        // Jitsi rooms are ephemeral - they're automatically cleaned up when empty
        // No deletion needed
        return Task.FromResult(true);
    }

    private string GenerateRoomId(string eventSlug, Guid? bookingId)
    {
        // Create a URL-safe room ID
        var slug = SanitizeForUrl(eventSlug);
        var uniqueId = bookingId?.ToString("N")[..8] ?? Guid.NewGuid().ToString("N")[..8];
        return $"{_settings.RoomPrefix}-{slug}-{uniqueId}";
    }

    private static string SanitizeForUrl(string input)
    {
        // Remove special characters and spaces, keep alphanumeric and hyphens
        var sanitized = new string(input
            .ToLowerInvariant()
            .Select(c => char.IsLetterOrDigit(c) ? c : '-')
            .ToArray());

        // Remove consecutive hyphens and trim
        while (sanitized.Contains("--"))
        {
            sanitized = sanitized.Replace("--", "-");
        }

        return sanitized.Trim('-');
    }
}
