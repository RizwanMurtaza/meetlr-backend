using Meetlr.Application.Features.MeetlrEvents.Commands.CreateOrUpdateTheme;

namespace Meetlr.Api.Endpoints.EventThemes.CreateOrUpdate;

public class CreateOrUpdateEventThemeRequest
{
    // Colors
    public string PrimaryColor { get; set; } = "#6366f1";
    public string SecondaryColor { get; set; } = "#8b5cf6";
    public string CalendarBackgroundColor { get; set; } = "#ffffff";
    public string TextColor { get; set; } = "#1f2937";

    // Typography
    public string FontFamily { get; set; } = "Inter";
    public string ButtonStyle { get; set; } = "rounded";
    public int BorderRadius { get; set; } = 8;

    // Banner/Content
    public string? BannerImageUrl { get; set; }
    public string? BannerText { get; set; }
    public bool ShowHostPhoto { get; set; } = true;
    public bool ShowEventDescription { get; set; } = true;
    public string? CustomWelcomeMessage { get; set; }

    public static CreateOrUpdateEventThemeCommand ToCommand(Guid eventId, CreateOrUpdateEventThemeRequest request)
    {
        return new CreateOrUpdateEventThemeCommand
        {
            MeetlrEventId = eventId,
            PrimaryColor = request.PrimaryColor,
            SecondaryColor = request.SecondaryColor,
            CalendarBackgroundColor = request.CalendarBackgroundColor,
            TextColor = request.TextColor,
            FontFamily = request.FontFamily,
            ButtonStyle = request.ButtonStyle,
            BorderRadius = request.BorderRadius,
            BannerImageUrl = request.BannerImageUrl,
            BannerText = request.BannerText,
            ShowHostPhoto = request.ShowHostPhoto,
            ShowEventDescription = request.ShowEventDescription,
            CustomWelcomeMessage = request.CustomWelcomeMessage
        };
    }
}
