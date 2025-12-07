using Meetlr.Domain.Exceptions.Base;
using Meetlr.Domain.Exceptions.Http;

namespace Meetlr.Domain.Exceptions.DomainErrors.ExceptionByArea;

/// <summary>
/// Defines errors related to availability management.
/// All errors use ApplicationArea.Availability (area code: 7)
/// </summary>
public static class AvailabilityErrors
{
    private const ApplicationArea Area = ApplicationArea.Availability;

    public static NotFoundException AvailabilityScheduleNotFound(Guid scheduleId, string? customMessage = null)
    {
        var exception = new NotFoundException(Area, 1, customMessage ?? "Availability schedule not found");
        exception.WithDetail("ScheduleId", scheduleId);
        return exception;
    }

    public static BadRequestException InvalidTimeRange(TimeSpan startTime, TimeSpan endTime, string? customMessage = null)
    {
        var exception = new BadRequestException(Area, 2, customMessage ?? "End time must be after start time");
        exception.WithDetail("StartTime", startTime);
        exception.WithDetail("EndTime", endTime);
        return exception;
    }

    public static BadRequestException DateOverrideAlreadyExists(DateTime date, string? customMessage = null)
    {
        var exception = new BadRequestException(Area, 3, customMessage ?? "A date override already exists for this date");
        exception.WithDetail("Date", date);
        return exception;
    }

    public static BadRequestException OutsideAvailableHours(string? customMessage = null)
        => new(Area, 4, customMessage ?? "The requested time is outside available hours");

    public static ForbiddenException NotScheduleOwner(string? customMessage = null)
        => new(Area, 5, customMessage ?? "You do not have permission to modify this availability schedule");

    public static BadRequestException NoAvailableSlots(DateTime startDate, DateTime endDate, string? customMessage = null)
    {
        var exception = new BadRequestException(Area, 6, customMessage ?? "No available time slots found in the requested date range");
        exception.WithDetail("StartDate", startDate);
        exception.WithDetail("EndDate", endDate);
        return exception;
    }

    public static ForbiddenException ScheduleNotFoundOrNoPermission(string? customMessage = null)
        => new(Area, 7, customMessage ?? "Schedule not found or you don't have permission to update it");

    public static ConflictException TimezoneScheduleAlreadyExists(string timezone, string? customMessage = null)
    {
        var exception = new ConflictException(Area, 8, customMessage ?? $"A schedule with timezone '{timezone}' already exists. You can only have one schedule per timezone.");
        exception.WithDetail("Timezone", timezone);
        return exception;
    }

    public static ConflictException ScheduleInUse(string? customMessage = null)
        => new(Area, 9, customMessage ?? "Cannot delete this schedule because it is being used by one or more event types. Please update those event types first");

    public static ConflictException SingleScheduleLimitReached(string? customMessage = null)
        => new(Area, 10, customMessage ?? "You can only have one availability schedule. Please edit your existing schedule instead of creating a new one");

    public static ConflictException MaxScheduleLimitReached(int maxSchedules, string? customMessage = null)
    {
        var exception = new ConflictException(Area, 11, customMessage ?? $"You can have a maximum of {maxSchedules} availability schedules. Please edit or delete an existing schedule.");
        exception.WithDetail("MaxSchedules", maxSchedules);
        return exception;
    }

    public static ForbiddenException CalendarIntegrationNotFoundOrNoPermission(string? customMessage = null)
        => new(Area, 12, customMessage ?? "Calendar integration not found or you don't have permission to link it");
}
