using Meetlr.Domain.Exceptions.Base;
using Meetlr.Domain.Exceptions.Http;

namespace Meetlr.Domain.Exceptions.DomainErrors.ExceptionByArea;

/// <summary>
/// Defines errors related to Meetlr event management.
/// All errors use ApplicationArea.EventTypes (area code: 5)
/// </summary>
public static class MeetlrEventErrors
{
    private const ApplicationArea Area = ApplicationArea.EventTypes;

    public static NotFoundException MeetlrEventNotFound(Guid meetlrEventId, string? customMessage = null)
    {
        var exception = new NotFoundException(Area, 1, customMessage ?? "Meetlr event not found");
        exception.WithDetail("MeetlrEventId", meetlrEventId);
        return exception;
    }

    public static NotFoundException MeetlrEventNotFoundBySlug(string slug, string? customMessage = null)
    {
        var exception = new NotFoundException(Area, 2, customMessage ?? "Meetlr event not found");
        exception.WithDetail("Slug", slug);
        return exception;
    }

    public static ConflictException SlugAlreadyExists(string slug, string? customMessage = null)
    {
        var exception = new ConflictException(Area, 3, customMessage ?? "A Meetlr event with this slug already exists");
        exception.WithDetail("Slug", slug);
        return exception;
    }

    public static BadRequestException MeetlrEventNotActive(Guid meetlrEventId, string? customMessage = null)
    {
        var exception = new BadRequestException(Area, 4, customMessage ?? "This Meetlr event is not available for booking");
        exception.WithDetail("MeetlrEventId", meetlrEventId);
        return exception;
    }

    public static BadRequestException InvalidDuration(int minutes, string? customMessage = null)
    {
        var exception = new BadRequestException(Area, 5, customMessage ?? "Invalid event duration");
        exception.WithDetail("DurationMinutes", minutes);
        return exception;
    }

    public static ForbiddenException NotMeetlrEventOwner(string? customMessage = null)
        => new(Area, 6, customMessage ?? "You do not have permission to modify this Meetlr event");

    // Alias methods for backwards compatibility with code that still references old names
    public static NotFoundException EventTypeNotFound(Guid eventTypeId, string? customMessage = null)
        => MeetlrEventNotFound(eventTypeId, customMessage);

    public static BadRequestException EventTypeNotActive(Guid eventTypeId, string? customMessage = null)
        => MeetlrEventNotActive(eventTypeId, customMessage);

    public static ForbiddenException NotEventTypeOwner(string? customMessage = null)
        => NotMeetlrEventOwner(customMessage);
}
