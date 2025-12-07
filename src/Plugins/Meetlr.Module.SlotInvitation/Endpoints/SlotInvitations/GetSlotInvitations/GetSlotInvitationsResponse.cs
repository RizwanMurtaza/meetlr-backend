using Meetlr.Module.SlotInvitation.Application.Queries.GetSlotInvitations;

namespace Meetlr.Module.SlotInvitation.Endpoints.SlotInvitations.GetSlotInvitations;

public record GetSlotInvitationsResponse
{
    public List<SlotInvitationDto> Invitations { get; init; } = new();
    public int TotalCount { get; init; }
    public int PageNumber { get; init; }
    public int PageSize { get; init; }
    public int TotalPages { get; init; }
    public bool HasPreviousPage { get; init; }
    public bool HasNextPage { get; init; }
}
