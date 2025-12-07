namespace Meetlr.Domain.Enums;

/// <summary>
/// Defines the type of meeting for a Meetlr event
/// </summary>
public enum MeetingType
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
    Group = 2,

    /// <summary>
    /// FullDay: An all-day event on a specific date (no time selection)
    /// Use case: Workshops, conferences, full-day training sessions
    /// Similar to Group but spans the entire day - invitees select a date only
    /// </summary>
    FullDay = 3,

    /// <summary>
    /// OneOff: A single occurrence event on a specific date (no time selection)
    /// Use case: Special events, one-time workshops, specific date appointments
    /// Similar to Group but for a specific date only - invitees select the date
    /// </summary>
    OneOff = 4
}
