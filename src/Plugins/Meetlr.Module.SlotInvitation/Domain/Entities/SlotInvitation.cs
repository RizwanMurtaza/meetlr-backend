using Meetlr.Domain.Common;
using Meetlr.Module.SlotInvitation.Domain.Enums;

namespace Meetlr.Module.SlotInvitation.Domain.Entities;

/// <summary>
/// Represents a slot invitation where a host offers a specific time slot to an invitee.
/// The slot is held/reserved until the invitation expires or the invitee books it.
/// </summary>
public class SlotInvitation : BaseAuditableEntity
{
    // Note: TenantId is inherited from BaseEntity

    /// <summary>
    /// The host user who created this invitation
    /// </summary>
    public Guid UserId { get; set; }

    /// <summary>
    /// The event type for this invitation
    /// </summary>
    public Guid MeetlrEventId { get; set; }

    /// <summary>
    /// Optional - existing contact from the contacts table
    /// </summary>
    public Guid? ContactId { get; set; }

    // Slot Details

    /// <summary>
    /// Start time of the reserved slot (UTC)
    /// </summary>
    public DateTime SlotStartTime { get; set; }

    /// <summary>
    /// End time of the reserved slot (UTC)
    /// </summary>
    public DateTime SlotEndTime { get; set; }

    /// <summary>
    /// Number of spots reserved for group events (default: 1)
    /// </summary>
    public int SpotsReserved { get; set; } = 1;

    // Invitation Details

    /// <summary>
    /// Secure token for the invitation URL
    /// </summary>
    public string Token { get; set; } = string.Empty;

    /// <summary>
    /// Email address of the invitee
    /// </summary>
    public string InviteeEmail { get; set; } = string.Empty;

    /// <summary>
    /// Optional name of the invitee
    /// </summary>
    public string? InviteeName { get; set; }

    /// <summary>
    /// When this invitation expires (UTC)
    /// </summary>
    public DateTime ExpiresAt { get; set; }

    /// <summary>
    /// Number of hours until expiration (1-72)
    /// </summary>
    public int ExpirationHours { get; set; }

    // Status Tracking

    /// <summary>
    /// Current status of the invitation
    /// </summary>
    public SlotInvitationStatus Status { get; set; } = SlotInvitationStatus.Pending;

    /// <summary>
    /// Booking ID if the invitation was booked
    /// </summary>
    public Guid? BookingId { get; set; }

    /// <summary>
    /// When the booking was completed
    /// </summary>
    public DateTime? BookedAt { get; set; }

    // Email Tracking

    /// <summary>
    /// Current email delivery status
    /// </summary>
    public EmailDeliveryStatus EmailStatus { get; set; } = EmailDeliveryStatus.Queued;

    /// <summary>
    /// Number of email send attempts
    /// </summary>
    public int EmailAttempts { get; set; }

    /// <summary>
    /// When the email was successfully sent
    /// </summary>
    public DateTime? EmailSentAt { get; set; }

    /// <summary>
    /// Last error message if email failed
    /// </summary>
    public string? EmailError { get; set; }

    // Helper Methods

    /// <summary>
    /// Check if the invitation is still valid (pending and not expired)
    /// </summary>
    public bool IsValid => Status == SlotInvitationStatus.Pending && ExpiresAt > DateTime.UtcNow;

    /// <summary>
    /// Check if the invitation can be deleted (only if not booked)
    /// </summary>
    public bool CanBeDeleted => Status != SlotInvitationStatus.Booked;

    /// <summary>
    /// Check if the email can be resent (max 3 attempts)
    /// </summary>
    public bool CanResendEmail => Status == SlotInvitationStatus.Pending && EmailAttempts < 3;

    /// <summary>
    /// Mark the invitation as booked
    /// </summary>
    public void MarkAsBooked(Guid bookingId)
    {
        Status = SlotInvitationStatus.Booked;
        BookingId = bookingId;
        BookedAt = DateTime.UtcNow;
    }

    /// <summary>
    /// Mark the invitation as expired
    /// </summary>
    public void MarkAsExpired()
    {
        if (Status == SlotInvitationStatus.Pending)
        {
            Status = SlotInvitationStatus.Expired;
        }
    }

    /// <summary>
    /// Mark the invitation as cancelled
    /// </summary>
    public void MarkAsCancelled()
    {
        if (CanBeDeleted)
        {
            Status = SlotInvitationStatus.Cancelled;
        }
    }

    /// <summary>
    /// Update email status after send attempt
    /// </summary>
    public void UpdateEmailStatus(bool success, string? error = null)
    {
        EmailAttempts++;
        if (success)
        {
            EmailStatus = EmailDeliveryStatus.Sent;
            EmailSentAt = DateTime.UtcNow;
            EmailError = null;
        }
        else
        {
            EmailStatus = EmailDeliveryStatus.Failed;
            EmailError = error;
        }
    }
}
