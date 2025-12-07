namespace Meetlr.Domain.Enums;

public enum NotificationType
{
    Email = 0,
    Sms = 1,
    WhatsApp = 2,
    Refund = 3,
    VideoLinkCreation = 4,    // Create video meeting link (Zoom, Teams, Google Meet, Jitsi)
    CalendarSync = 5,         // Create calendar event
    VideoLinkDeletion = 6,    // Delete video meeting link
    CalendarDeletion = 7,     // Delete calendar event
    CalendarReschedule = 8,   // Reschedule: delete old calendar event + create new one
    SlotInvitationEmail = 9   // Send slot invitation email to invitee
}
