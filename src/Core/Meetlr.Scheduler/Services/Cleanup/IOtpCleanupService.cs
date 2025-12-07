namespace Meetlr.Scheduler.Services.Cleanup;

/// <summary>
/// Service for cleaning up expired OTPs.
/// </summary>
public interface IOtpCleanupService
{
    /// <summary>
    /// Removes OTPs that have expired.
    /// </summary>
    Task CleanupExpiredOtpsAsync(CancellationToken cancellationToken);
}
