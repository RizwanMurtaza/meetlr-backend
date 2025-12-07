namespace Meetlr.Domain.Enums;

/// <summary>
/// Defines the type of meeting for an event type
/// </summary>
public enum EventTypeMeetingType
{
    /// <summary>
    /// One-on-One: A meeting between you and one invitee
    /// Use case: Individual consultations, interviews, sales calls, 1:1 meetings
    /// </summary>
    OneOnOne = 1,
    
    /// <summary>
    /// Group: Multiple invitees can book the same time slot together
    /// Use case: Webinars, training sessions, group classes, office hours
    /// Requires: MaxAttendeesPerSlot to be set (capacity limit)
    /// </summary>
    Group = 2
}
