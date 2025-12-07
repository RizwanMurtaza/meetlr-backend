using Meetlr.Application.Features.MeetlrEvents.Queries.GetTypesByUsername;

namespace Meetlr.Api.Endpoints.Public.GetByUsername;

public record GetMeetlrEventsByUsernameResponse
{
    public List<MeetlrEventListItemDto> MeetlrEvents { get; init; } = new();
    public HostInfoDto? Host { get; init; }

    public static GetMeetlrEventsByUsernameResponse FromQueryResponse(GetMeetlrEventsByUsernameQueryResponse response)
    {
        return new GetMeetlrEventsByUsernameResponse
        {
            MeetlrEvents = response.MeetlrEvents.Select(e => new MeetlrEventListItemDto
            {
                Id = e.Id,
                Name = e.Name,
                Slug = e.Slug,
                Description = e.Description,
                DurationMinutes = e.DurationMinutes,
                Color = e.Color,
                Location = e.Location,
                Fee = e.Fee,
                Currency = e.Currency,
                AllowsRecurring = e.AllowsRecurring,
                MaxRecurringOccurrences = e.MaxRecurringOccurrences,
                IsActive = e.IsActive
            }).ToList(),
            Host = response.Host != null ? new HostInfoDto
            {
                FirstName = response.Host.FirstName,
                LastName = response.Host.LastName,
                CompanyName = response.Host.CompanyName,
                LogoUrl = response.Host.LogoUrl
            } : null
        };
    }
}