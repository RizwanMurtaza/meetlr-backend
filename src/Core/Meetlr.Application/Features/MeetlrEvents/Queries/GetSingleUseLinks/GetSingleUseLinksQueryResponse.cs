namespace Meetlr.Application.Features.MeetlrEvents.Queries.GetSingleUseLinks;

public class GetSingleUseLinksQueryResponse
{
    public List<SingleUseLinkItem> Links { get; set; } = new();
}

public class SingleUseLinkItem
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
