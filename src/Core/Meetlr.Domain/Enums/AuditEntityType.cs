namespace Meetlr.Domain.Enums;

/// <summary>
/// Entity types that can be audited in the system
/// </summary>
public enum AuditEntityType
{
    User = 1,
    Tenant = 2,
    Group = 3,
    MeetlrEvent = 4,
    Booking = 5,
    BookingSeries = 6,
    AvailabilitySchedule = 7,
    DateOverride = 8,
    CalendarIntegration = 9,
    Plugin = 10,
    Payment = 11
}
