namespace Meetlr.Module.Calendar.Application.Queries.GetConnectedCalendars;

public record CalendarIntegrationDto(
    Guid Id,
    Guid AvailabilityScheduleId,
    string Provider,
    string Email,
    bool IsConnected,
    DateTime? LastSync,
    bool IsPrimaryCalendar,
    bool CheckForConflicts,
    bool AddEventsToCalendar,
    bool IncludeBuffers,
    bool AutoSync
);
