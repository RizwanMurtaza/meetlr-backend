using MediatR;

namespace Meetlr.Application.Features.MeetlrEvents.Commands.CreateOrUpdateTheme;

public class CreateOrUpdateEventThemeCommand : IRequest<CreateOrUpdateEventThemeResponse>
{
    public Guid MeetlrEventId { get; set; }

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
}
