using Microsoft.AspNetCore.Mvc;

namespace Meetlr.Module.SlotInvitation.Endpoints.SlotInvitations.GetSlotInvitations;

public record GetSlotInvitationsRequest
{
    /// <summary>
    /// Required - filter invitations by specific event
    /// </summary>
    [FromRoute]
    public Guid MeetlrEventId { get; init; }

    /// <summary>
    /// Page number (1-based)
    /// </summary>
    [FromQuery]
    public int PageNumber { get; init; } = 1;

    /// <summary>
    /// Page size
    /// </summary>
    [FromQuery]
    public int PageSize { get; init; } = 20;

    /// <summary>
    /// Optional status filter (0=Pending, 1=Booked, 2=Expired, 3=Cancelled)
    /// </summary>
    [FromQuery]
    public int? Status { get; init; }

    /// <summary>
    /// Sort by field (default: CreatedAt)
    /// </summary>
    [FromQuery]
    public string? SortBy { get; init; } = "CreatedAt";

    /// <summary>
    /// Sort descending (default: true - newest first)
    /// </summary>
    [FromQuery]
    public bool SortDescending { get; init; } = true;
}
