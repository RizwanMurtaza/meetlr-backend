namespace Meetlr.Api.Endpoints.EventThemes.CreateOrUpdate;

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

    public static CreateOrUpdateEventThemeResponse FromCommandResponse(
        Application.Features.MeetlrEvents.Commands.CreateOrUpdateTheme.CreateOrUpdateEventThemeResponse response)
    {
        return new CreateOrUpdateEventThemeResponse
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
