namespace Meetlr.Module.Analytics.Domain.Enums;

/// <summary>
/// Types of pages that can be tracked for analytics
/// </summary>
public enum PageViewType
{
    /// <summary>
    /// User's public homepage (/site/[username])
    /// </summary>
    UserHomepage = 0,

    /// <summary>
    /// User's event list page (/book/[username])
    /// </summary>
    EventList = 1,

    /// <summary>
    /// Individual event page (/book/[username]/[slug])
    /// </summary>
    EventPage = 2
}
