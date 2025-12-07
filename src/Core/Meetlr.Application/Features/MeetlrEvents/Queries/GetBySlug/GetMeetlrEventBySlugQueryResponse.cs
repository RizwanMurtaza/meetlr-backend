using Meetlr.Domain.Enums;

namespace Meetlr.Application.Features.MeetlrEvents.Queries.GetBySlug;

public class GetMeetlrEventBySlugQueryResponse
{
    public Guid Id { get; set; }
    public Guid TenantId { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Slug { get; set; } = string.Empty;
    public string? Description { get; set; }
    public int DurationMinutes { get; set; }
    public int BufferTimeBeforeMinutes { get; set; }
    public int BufferTimeAfterMinutes { get; set; }
    public int MinBookingNoticeMinutes { get; set; }
    public string? Color { get; set; }
    public decimal? Fee { get; set; }
    public string? Currency { get; set; }
    public bool AllowsRecurring { get; set; }
    public int? MaxRecurringOccurrences { get; set; }
    public MeetingType MeetingType { get; set; }
    public List<QuestionDto> Questions { get; set; } = new();
    public HostDto Host { get; set; } = new();
    public EventThemeDto? Theme { get; set; }
}

public class EventThemeDto
{
    public string PrimaryColor { get; set; } = "#6366f1";
    public string SecondaryColor { get; set; } = "#8b5cf6";
    public string CalendarBackgroundColor { get; set; } = "#ffffff";
    public string TextColor { get; set; } = "#1f2937";
    public string FontFamily { get; set; } = "Inter";
    public string ButtonStyle { get; set; } = "rounded";
    public int BorderRadius { get; set; } = 8;
    public string? BannerImageUrl { get; set; }
    public string? BannerText { get; set; }
    public bool ShowHostPhoto { get; set; } = true;
    public bool ShowEventDescription { get; set; } = true;
    public string? CustomWelcomeMessage { get; set; }
}

