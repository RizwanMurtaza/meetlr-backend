namespace Meetlr.Module.Analytics.Application.Commands.TrackPageView;

/// <summary>
/// Response for the TrackPageView command
/// </summary>
public record TrackPageViewCommandResponse
{
    /// <summary>
    /// Indicates if the page view was tracked successfully
    /// </summary>
    public bool Success { get; init; }

    /// <summary>
    /// The ID of the created page view record (for debugging)
    /// </summary>
    public Guid? PageViewId { get; init; }
}
