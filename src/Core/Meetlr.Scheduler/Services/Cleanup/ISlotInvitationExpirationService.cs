namespace Meetlr.Scheduler.Services.Cleanup;

/// <summary>
/// Service for marking expired slot invitations as expired.
/// This frees up reserved time slots for other bookings.
/// </summary>
public interface ISlotInvitationExpirationService
{
    /// <summary>
    /// Mark all pending slot invitations that have passed their expiration time as expired.
    /// </summary>
    Task MarkExpiredInvitationsAsync(CancellationToken cancellationToken);
}
