using Meetlr.Application.Features.MeetlrEvents.Queries.GetSingleUseLinks;

namespace Meetlr.Api.Endpoints.SingleUseLinks.Get;

public class GetSingleUseLinksResponse
{
    public List<SingleUseLinkDto> Links { get; set; } = new();

    public static GetSingleUseLinksResponse FromQueryResponse(GetSingleUseLinksQueryResponse queryResponse)
    {
        return new GetSingleUseLinksResponse
        {
            Links = queryResponse.Links.Select(l => new SingleUseLinkDto
            {
                Id = l.Id,
                Token = l.Token,
                BookingUrl = l.BookingUrl,
                Name = l.Name,
                IsUsed = l.IsUsed,
                UsedAt = l.UsedAt,
                ExpiresAt = l.ExpiresAt,
                IsExpired = l.IsExpired,
                IsActive = l.IsActive,
                CreatedAt = l.CreatedAt
            }).ToList()
        };
    }
}

public class SingleUseLinkDto
{
    public Guid Id { get; set; }
    public string Token { get; set; } = string.Empty;
    public string BookingUrl { get; set; } = string.Empty;
    public string? Name { get; set; }
    public bool IsUsed { get; set; }
    public DateTime? UsedAt { get; set; }
    public DateTime? ExpiresAt { get; set; }
    public bool IsExpired { get; set; }
    public bool IsActive { get; set; }
    public DateTime CreatedAt { get; set; }
}
