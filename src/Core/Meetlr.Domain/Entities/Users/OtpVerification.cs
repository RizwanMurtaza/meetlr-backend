using Meetlr.Domain.Common;
using Meetlr.Domain.Enums;

namespace Meetlr.Domain.Entities.Users;

/// <summary>
/// One-Time Password verification entity for email verification, password reset, and 2FA
/// </summary>
public class OtpVerification : BaseAuditableEntity
{
    /// <summary>
    /// User ID this OTP belongs to
    /// </summary>
    public Guid UserId { get; set; }

    /// <summary>
    /// 6-digit OTP code
    /// </summary>
    public string Code { get; set; } = string.Empty;

    /// <summary>
    /// Purpose of this OTP (email verification, password reset, 2FA)
    /// </summary>
    public OtpPurpose Purpose { get; set; }

    /// <summary>
    /// When this OTP expires (5 minutes from creation)
    /// </summary>
    public DateTime ExpiresAt { get; set; }

    /// <summary>
    /// Whether this OTP has been used
    /// </summary>
    public bool IsUsed { get; set; } = false;

    /// <summary>
    /// When this OTP was used
    /// </summary>
    public DateTime? UsedAt { get; set; }

    /// <summary>
    /// Number of failed verification attempts
    /// </summary>
    public int AttemptCount { get; set; } = 0;

    /// <summary>
    /// Last attempt timestamp (for rate limiting)
    /// </summary>
    public DateTime? LastAttemptAt { get; set; }

    // Navigation property
    public User User { get; set; } = null!;
}
