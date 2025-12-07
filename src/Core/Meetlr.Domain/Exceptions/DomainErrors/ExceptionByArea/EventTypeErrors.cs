using Meetlr.Domain.Exceptions.Base;
using Meetlr.Domain.Exceptions.Http;

namespace Meetlr.Domain.Exceptions.DomainErrors.ExceptionByArea;

/// <summary>
/// Defines errors related to event type management.
/// All errors use ApplicationArea.EventTypes (area code: 5)
/// </summary>
public static class EventTypeErrors
{
    private const ApplicationArea Area = ApplicationArea.EventTypes;

    public static NotFoundException EventTypeNotFound(Guid eventTypeId, string? customMessage = null)
    {
        var exception = new NotFoundException(Area, 1, customMessage ?? "Event type not found");
        exception.WithDetail("EventTypeId", eventTypeId);
        return exception;
    }

    public static NotFoundException EventTypeNotFoundBySlug(string slug, string? customMessage = null)
    {
        var exception = new NotFoundException(Area, 2, customMessage ?? "Event type not found");
        exception.WithDetail("Slug", slug);
        return exception;
    }

    public static ConflictException SlugAlreadyExists(string slug, string? customMessage = null)
    {
        var exception = new ConflictException(Area, 3, customMessage ?? "An event type with this slug already exists");
        exception.WithDetail("Slug", slug);
        return exception;
    }

    public static BadRequestException EventTypeNotActive(Guid eventTypeId, string? customMessage = null)
    {
        var exception = new BadRequestException(Area, 4, customMessage ?? "This event type is not available for booking");
        exception.WithDetail("EventTypeId", eventTypeId);
        return exception;
    }

    public static BadRequestException InvalidDuration(int minutes, string? customMessage = null)
    {
        var exception = new BadRequestException(Area, 5, customMessage ?? "Invalid event duration");
        exception.WithDetail("DurationMinutes", minutes);
        return exception;
    }

    public static ForbiddenException NotEventTypeOwner(string? customMessage = null)
        => new(Area, 6, customMessage ?? "You do not have permission to modify this event type");

    public static BadRequestException AvailabilityScheduleRequired(string? customMessage = null)
        => new(Area, 7, customMessage ?? "AvailabilityScheduleId is required for this event type");
}
