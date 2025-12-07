using Meetlr.Domain.Enums;

namespace Meetlr.Application.Interfaces;

/// <summary>
/// Service for generating and validating one-time passwords (OTPs)
/// Supports email verification, password reset, and 2FA
/// </summary>
public interface IOtpService
{
    /// <summary>
    /// Generate a new 6-digit OTP for a user
    /// Invalidates any existing unused OTPs for the same purpose
    /// </summary>
    /// <param name="userId">User ID</param>
    /// <param name="purpose">Purpose of the OTP (email verification, password reset, 2FA)</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Generated 6-digit OTP code</returns>
    Task<string> GenerateOtpAsync(
        Guid userId,
        OtpPurpose purpose,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Validate an OTP code
    /// Checks: code matches, not expired, not used, not exceeded max attempts
    /// Marks OTP as used upon successful validation
    /// </summary>
    /// <param name="userId">User ID</param>
    /// <param name="code">6-digit OTP code</param>
    /// <param name="purpose">Purpose of the OTP</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>True if valid and marked as used, false if invalid</returns>
    Task<bool> ValidateOtpAsync(
        Guid userId,
        string code,
        OtpPurpose purpose,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Invalidate all unused OTPs for a user and purpose
    /// Useful when generating a new OTP or user requests cancellation
    /// </summary>
    /// <param name="userId">User ID</param>
    /// <param name="purpose">Purpose of the OTPs to invalidate</param>
    /// <param name="cancellationToken">Cancellation token</param>
    Task InvalidateOtpsAsync(
        Guid userId,
        OtpPurpose purpose,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Cleanup expired OTPs (soft delete)
    /// Should be called periodically by background service
    /// </summary>
    /// <param name="cancellationToken">Cancellation token</param>
    Task CleanupExpiredOtpsAsync(CancellationToken cancellationToken = default);
}
