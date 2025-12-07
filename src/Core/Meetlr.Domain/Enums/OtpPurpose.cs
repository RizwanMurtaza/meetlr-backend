namespace Meetlr.Domain.Enums;

/// <summary>
/// Purpose/reason for OTP generation
/// </summary>
public enum OtpPurpose
{
    /// <summary>
    /// OTP for email verification during registration
    /// </summary>
    EmailVerification = 1,

    /// <summary>
    /// OTP for password reset
    /// </summary>
    PasswordReset = 2,

    /// <summary>
    /// OTP for two-factor authentication login
    /// </summary>
    TwoFactorAuth = 3
}
