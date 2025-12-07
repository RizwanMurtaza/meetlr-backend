using MediatR;

namespace Meetlr.Module.SlotInvitation.Application.Queries.GetSlotInvitationByToken;

/// <summary>
/// Query to get slot invitation details by token.
/// Used by the public booking page to display invitation details.
/// </summary>
public record GetSlotInvitationByTokenQuery : IRequest<GetSlotInvitationByTokenQueryResponse?>
{
    public string Token { get; init; } = string.Empty;
}

public record GetSlotInvitationByTokenQueryResponse
{
    public Guid Id { get; init; }
    public Guid TenantId { get; init; }
    public Guid MeetlrEventId { get; init; }
    public string MeetlrEventName { get; init; } = string.Empty;
    public string MeetlrEventSlug { get; init; } = string.Empty;
    public string HostName { get; init; } = string.Empty;
    public string HostUsername { get; init; } = string.Empty;
    public DateTime SlotStartTime { get; init; }
    public DateTime SlotEndTime { get; init; }
    public int DurationMinutes { get; init; }
    public int SpotsReserved { get; init; }
    public string? InviteeEmail { get; init; }
    public string? InviteeName { get; init; }
    public DateTime ExpiresAt { get; init; }
    public string Status { get; init; } = string.Empty;
    public bool IsValid { get; init; }
}
