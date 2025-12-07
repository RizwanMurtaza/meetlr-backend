using Meetlr.Module.Analytics.Application.Commands.TrackPageView;

namespace Meetlr.Module.Analytics.Endpoints.Public.TrackPageView;

/// <summary>
/// API response model for tracking a page view
/// </summary>
public record TrackPageViewResponse
{
    /// <summary>
    /// Indicates if the page view was tracked successfully
    /// </summary>
    public bool Success { get; init; }

    /// <summary>
    /// Create from command response
    /// </summary>
    public static TrackPageViewResponse FromCommandResponse(TrackPageViewCommandResponse commandResponse) => new()
    {
        Success = commandResponse.Success
    };
}
