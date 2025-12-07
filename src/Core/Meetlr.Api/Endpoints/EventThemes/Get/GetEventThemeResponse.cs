namespace Meetlr.Api.Endpoints.EventThemes.Get;

public class GetEventThemeResponse
{
    public Guid? Id { get; set; }
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

    public DateTime? CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }

    /// <summary>
    /// Returns a default theme when no custom theme exists
    /// </summary>
    public static GetEventThemeResponse GetDefault(Guid eventId)
    {
        return new GetEventThemeResponse
        {
            Id = null,
            MeetlrEventId = eventId,
            PrimaryColor = "#6366f1",
            SecondaryColor = "#8b5cf6",
            CalendarBackgroundColor = "#ffffff",
            TextColor = "#1f2937",
            FontFamily = "Inter",
            ButtonStyle = "rounded",
            BorderRadius = 8,
            BannerImageUrl = null,
            BannerText = null,
            ShowHostPhoto = true,
            ShowEventDescription = true,
            CustomWelcomeMessage = null,
            CreatedAt = null,
            UpdatedAt = null
        };
    }

    public static GetEventThemeResponse FromQueryResponse(
        Application.Features.MeetlrEvents.Queries.GetTheme.GetEventThemeResponse response)
    {
        return new GetEventThemeResponse
        {
            Id = response.Id,
            MeetlrEventId = response.MeetlrEventId,
            PrimaryColor = response.PrimaryColor,
            SecondaryColor = response.SecondaryColor,
            CalendarBackgroundColor = response.CalendarBackgroundColor,
            TextColor = response.TextColor,
            FontFamily = response.FontFamily,
            ButtonStyle = response.ButtonStyle,
            BorderRadius = response.BorderRadius,
            BannerImageUrl = response.BannerImageUrl,
            BannerText = response.BannerText,
            ShowHostPhoto = response.ShowHostPhoto,
            ShowEventDescription = response.ShowEventDescription,
            CustomWelcomeMessage = response.CustomWelcomeMessage,
            CreatedAt = response.CreatedAt,
            UpdatedAt = response.UpdatedAt
        };
    }
}
