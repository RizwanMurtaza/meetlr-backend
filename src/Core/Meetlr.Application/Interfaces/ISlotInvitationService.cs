using Meetlr.Application.Models;

namespace Meetlr.Application.Interfaces;

/// <summary>
/// Service interface for the SlotInvitation module to expose slot reservations.
/// This allows the core Availability handlers to check for reserved slots
/// without depending on the SlotInvitation module directly.
///
/// When the SlotInvitation module is registered, it provides an implementation
/// of this interface. When the module is not registered, the interface is not
/// resolved and the availability handlers skip the reservation check.
/// </summary>
public interface ISlotInvitationService
{
    /// <summary>
    /// Get all active (pending, not expired) slot reservations for an event
    /// within the specified date range.
    /// </summary>
    /// <param name="meetlrEventId">The event ID to check reservations for</param>
    /// <param name="startDate">Start of the date range (UTC)</param>
    /// <param name="endDate">End of the date range (UTC)</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>List of active slot reservations</returns>
    Task<List<SlotReservationInfo>> GetActiveReservationsAsync(
        Guid meetlrEventId,
        DateTime startDate,
        DateTime endDate,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Get a slot invitation by its secure token.
    /// Used for validating booking requests with slot invitation tokens.
    /// </summary>
    /// <param name="token">The secure invitation token</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>The slot invitation details, or null if not found</returns>
    Task<SlotInvitationDto?> GetByTokenAsync(
        string token,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Mark a slot invitation as accepted when a booking is created.
    /// Updates the status to Accepted and links the booking ID.
    /// </summary>
    /// <param name="slotInvitationId">The slot invitation ID</param>
    /// <param name="bookingId">The created booking ID</param>
    /// <param name="cancellationToken">Cancellation token</param>
    Task MarkAsAcceptedAsync(
        Guid slotInvitationId,
        Guid bookingId,
        CancellationToken cancellationToken = default);
}

/// <summary>
/// DTO for slot invitation data used in booking validation
/// </summary>
public class SlotInvitationDto
{
    public Guid Id { get; set; }
    public Guid MeetlrEventId { get; set; }
    public DateTime SlotStartTime { get; set; }
    public DateTime SlotEndTime { get; set; }
    public int SpotsReserved { get; set; }
    public string Status { get; set; } = string.Empty;
    public DateTime ExpiresAt { get; set; }
    public string? InviteeEmail { get; set; }
    public string? InviteeName { get; set; }
}
