namespace Meetlr.Domain.Enums;

/// <summary>
/// Types of email templates available in the system
/// </summary>
public enum EmailTemplateType
{
    /// <summary>
    /// Welcome email sent after successful email verification
    /// </summary>
    WelcomeEmail = 1,

    /// <summary>
    /// Email verification with OTP code
    /// </summary>
    EmailVerification = 2,

    /// <summary>
    /// Booking confirmation sent to the host
    /// </summary>
    BookingConfirmationHost = 3,

    /// <summary>
    /// Booking confirmation sent to the invitee
    /// </summary>
    BookingConfirmationInvitee = 4,

    /// <summary>
    /// Booking cancellation notification sent to the hostt
    /// </summary>
    BookingCancellationHost = 5,

    /// <summary>
    /// Booking cancellation notification sent to the invitee
    /// </summary>
    BookingCancellationInvitee = 6,

    /// <summary>
    /// Reminder email sent before the booking
    /// </summary>
    BookingReminder = 7,

    /// <summary>
    /// Password reset email with reset link
    /// </summary>
    PasswordReset = 8,

    /// <summary>
    /// Email sent when a booking is rescheduled
    /// </summary>
    BookingRescheduled = 9,

    /// <summary>
    /// General purpose email sent to event invitees
    /// </summary>
    GeneralEmail = 10,

    /// <summary>
    /// Slot invitation email sent to invite someone to book a specific time slot
    /// </summary>
    SlotInvitation = 11
}
