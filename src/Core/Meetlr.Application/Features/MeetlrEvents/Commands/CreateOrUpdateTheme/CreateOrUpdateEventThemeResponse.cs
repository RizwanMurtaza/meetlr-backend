namespace Meetlr.Application.Features.MeetlrEvents.Commands.CreateOrUpdateTheme;

public class CreateOrUpdateEventThemeResponse
{
    public Guid Id { get; set; }
    public Guid MeetlrEventId { get; set; }

    // Colors
    public string PrimaryColor { get; set; } = string.Empty;
    public string SecondaryColor { get; set; } = string.Empty;
    public string CalendarBackgroundColor { get; set; } = string.Empty;
    public string TextColor { get; set; } = string.Empty;

    // Typography
    public string FontFamily { get; set; } = string.Empty;
    public string ButtonStyle { get; set; } = string.Empty;
    public int BorderRadius { get; set; }

    // Banner/Content
    public string? BannerImageUrl { get; set; }
    public string? BannerText { get; set; }
    public bool ShowHostPhoto { get; set; }
    public bool ShowEventDescription { get; set; }
    public string? CustomWelcomeMessage { get; set; }

    public DateTime CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }
}
