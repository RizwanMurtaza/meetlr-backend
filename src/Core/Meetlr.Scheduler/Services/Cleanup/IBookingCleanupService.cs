namespace Meetlr.Scheduler.Services.Cleanup;

/// <summary>
/// Service for cleaning up expired pending bookings.
/// </summary>
public interface IBookingCleanupService
{
    /// <summary>
    /// Cancels bookings that have been pending payment for too long.
    /// </summary>
    Task CleanupExpiredPendingBookingsAsync(CancellationToken cancellationToken);
}
