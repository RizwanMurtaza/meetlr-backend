namespace Meetlr.Module.SlotInvitation.Endpoints.SlotInvitations.CreateSlotInvitation;

public record CreateSlotInvitationRequest
{
    /// <summary>
    /// The event type for this invitation
    /// </summary>
    public Guid MeetlrEventId { get; init; }

    /// <summary>
    /// Optional - existing contact from the contacts table
    /// </summary>
    public Guid? ContactId { get; init; }

    /// <summary>
    /// Start time of the reserved slot (UTC)
    /// </summary>
    public DateTime SlotStartTime { get; init; }

    /// <summary>
    /// End time of the reserved slot (UTC). For full-day events, this should be end of day.
    /// </summary>
    public DateTime SlotEndTime { get; init; }

    /// <summary>
    /// Number of spots to reserve for group events (default: 1)
    /// </summary>
    public int SpotsReserved { get; init; } = 1;

    /// <summary>
    /// Email address of the invitee
    /// </summary>
    public string InviteeEmail { get; init; } = string.Empty;

    /// <summary>
    /// Optional name of the invitee
    /// </summary>
    public string? InviteeName { get; init; }

    /// <summary>
    /// Number of hours until expiration (1-72)
    /// </summary>
    public int ExpirationHours { get; init; } = 24;
}
