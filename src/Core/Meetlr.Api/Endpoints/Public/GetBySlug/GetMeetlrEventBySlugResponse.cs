using Meetlr.Application.Features.MeetlrEvents.Queries.GetBySlug;
using Meetlr.Domain.Enums;

namespace Meetlr.Api.Endpoints.Public.GetBySlug;

public class GetMeetlrEventBySlugResponse
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
    public List<QuestionItem> Questions { get; set; } = new();
    public HostInfo Host { get; set; } = new();
    public EventThemeInfo? Theme { get; set; }

    public static GetMeetlrEventBySlugResponse FromQueryResponse(GetMeetlrEventBySlugQueryResponse queryResponse)
    {
        return new GetMeetlrEventBySlugResponse
        {
            Id = queryResponse.Id,
            TenantId = queryResponse.TenantId,
            Name = queryResponse.Name,
            Slug = queryResponse.Slug,
            Description = queryResponse.Description,
            DurationMinutes = queryResponse.DurationMinutes,
            BufferTimeBeforeMinutes = queryResponse.BufferTimeBeforeMinutes,
            BufferTimeAfterMinutes = queryResponse.BufferTimeAfterMinutes,
            MinBookingNoticeMinutes = queryResponse.MinBookingNoticeMinutes,
            Color = queryResponse.Color,
            Fee = queryResponse.Fee,
            Currency = queryResponse.Currency,
            AllowsRecurring = queryResponse.AllowsRecurring,
            MaxRecurringOccurrences = queryResponse.MaxRecurringOccurrences,
            MeetingType = queryResponse.MeetingType,
            Questions = queryResponse.Questions.Select(q => new QuestionItem
            {
                Id = q.Id,
                Question = q.Question,
                Type = q.Type,
                IsRequired = q.IsRequired,
                Options = q.Options
            }).ToList(),
            Host = new HostInfo
            {
                FirstName = queryResponse.Host.FirstName,
                LastName = queryResponse.Host.LastName,
                CompanyName = queryResponse.Host.CompanyName,
                LogoUrl = queryResponse.Host.LogoUrl,
                ProfileImageUrl = queryResponse.Host.ProfileImageUrl,
                Bio = queryResponse.Host.Bio
            },
            Theme = queryResponse.Theme != null ? new EventThemeInfo
            {
                PrimaryColor = queryResponse.Theme.PrimaryColor,
                SecondaryColor = queryResponse.Theme.SecondaryColor,
                CalendarBackgroundColor = queryResponse.Theme.CalendarBackgroundColor,
                TextColor = queryResponse.Theme.TextColor,
                FontFamily = queryResponse.Theme.FontFamily,
                ButtonStyle = queryResponse.Theme.ButtonStyle,
                BorderRadius = queryResponse.Theme.BorderRadius,
                BannerImageUrl = queryResponse.Theme.BannerImageUrl,
                BannerText = queryResponse.Theme.BannerText,
                ShowHostPhoto = queryResponse.Theme.ShowHostPhoto,
                ShowEventDescription = queryResponse.Theme.ShowEventDescription,
                CustomWelcomeMessage = queryResponse.Theme.CustomWelcomeMessage
            } : null
        };
    }
}

public class EventThemeInfo
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
