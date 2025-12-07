using MediatR;

namespace Meetlr.Module.SlotInvitation.Application.Queries.GetSlotInvitations;

public record GetSlotInvitationsQuery : IRequest<GetSlotInvitationsQueryResponse>
{
    /// <summary>
    /// Required - filter invitations by specific event
    /// </summary>
    public Guid MeetlrEventId { get; init; }

    /// <summary>
    /// Page number (1-based)
    /// </summary>
    public int PageNumber { get; init; } = 1;

    /// <summary>
    /// Page size
    /// </summary>
    public int PageSize { get; init; } = 20;

    /// <summary>
    /// Optional status filter
    /// </summary>
    public int? Status { get; init; }

    /// <summary>
    /// Sort by field (default: CreatedAt)
    /// </summary>
    public string? SortBy { get; init; } = "CreatedAt";

    /// <summary>
    /// Sort descending (default: true - newest first)
    /// </summary>
    public bool SortDescending { get; init; } = true;
}
