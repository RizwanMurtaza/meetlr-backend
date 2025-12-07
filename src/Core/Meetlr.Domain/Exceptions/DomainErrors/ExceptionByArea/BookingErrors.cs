using Meetlr.Domain.Enums;
using Meetlr.Domain.Exceptions.Base;
using Meetlr.Domain.Exceptions.Http;

namespace Meetlr.Domain.Exceptions.DomainErrors.ExceptionByArea;

/// <summary>
/// Defines errors related to booking management.
/// All errors use ApplicationArea.Bookings (area code: 6)
/// </summary>
public static class BookingErrors
{
    private const ApplicationArea Area = ApplicationArea.Bookings;

    public static NotFoundException BookingNotFound(Guid bookingId, string? customMessage = null)
    {
        var exception = new NotFoundException(Area, 1, customMessage ?? "Booking not found");
        exception.WithDetail("BookingId", bookingId);
        return exception;
    }

    public static ConflictException TimeSlotConflict(DateTime startTime, DateTime endTime, string? customMessage = null)
    {
        var exception = new ConflictException(Area, 2, customMessage ?? "This time slot is no longer available");
        exception.WithDetail("StartTime", startTime);
        exception.WithDetail("EndTime", endTime);
        return exception;
    }

    public static BadRequestException PastBookingNotAllowed(string? customMessage = null)
        => new(Area, 3, customMessage ?? "Cannot book a time slot in the past");

    public static BadRequestException MinimumNoticeRequired(int minutes, string? customMessage = null)
    {
        var exception = new BadRequestException(Area, 4, customMessage ?? $"This event requires at least {minutes} minutes advance notice");
        exception.WithDetail("RequiredMinutes", minutes);
        return exception;
    }

    public static ConflictException BookingAlreadyCancelled(string? customMessage = null)
        => new(Area, 5, customMessage ?? "This booking has already been cancelled");

    public static BadRequestException InvalidCancellationToken(string? customMessage = null)
        => new(Area, 6, customMessage ?? "Invalid cancellation token");

    public static BadRequestException PaymentRequired(decimal amount, string? currency = null, string? customMessage = null)
    {
        var exception = new BadRequestException(Area, 7, customMessage ?? "Payment is required for this booking");
        exception.WithDetail("Amount", amount);
        if (currency != null)
            exception.WithDetail("Currency", currency);
        return exception;
    }

    public static BadRequestException OutsideBookingWindow(int maxDays, string? customMessage = null)
    {
        var exception = new BadRequestException(Area, 8, customMessage ?? $"Cannot book more than {maxDays} days in advance");
        exception.WithDetail("MaxBookingDaysInFuture", maxDays);
        return exception;
    }

    public static ForbiddenException NotBookingOwner(string? customMessage = null)
        => new(Area, 9, customMessage ?? "You do not have permission to access this booking");

    public static ConflictException GroupCapacityReached(int maxAttendees, string? customMessage = null)
    {
        var exception = new ConflictException(Area, 10, customMessage ?? $"This group session is full (maximum {maxAttendees} attendees)");
        exception.WithDetail("MaxAttendees", maxAttendees);
        return exception;
    }

    public static BadRequestException NoOccurrencesProvided(string? customMessage = null)
        => new(Area, 11, customMessage ?? "At least one booking date/time must be provided");

    public static BadRequestException TooManyOccurrences(int maxOccurrences, int providedCount, string? customMessage = null)
    {
        var exception = new BadRequestException(Area, 12, customMessage ?? $"Maximum {maxOccurrences} occurrences allowed per series");
        exception.WithDetail("MaxOccurrences", maxOccurrences);
        exception.WithDetail("ProvidedCount", providedCount);
        return exception;
    }

    public static NotFoundException SeriesNotFound(Guid seriesId, string? customMessage = null)
    {
        var exception = new NotFoundException(Area, 13, customMessage ?? "Booking series not found");
        exception.WithDetail("SeriesId", seriesId);
        return exception;
    }

    public static BadRequestException TenantIdRequired(string? customMessage = null)
        => new(Area, 14, customMessage ?? "TenantId is required for creating bookings");

    public static BadRequestException SeriesNotActive(Guid seriesId, string? customMessage = null)
    {
        var exception = new BadRequestException(Area, 15, customMessage ?? "Series is not in active state");
        exception.WithDetail("SeriesId", seriesId);
        return exception;
    }

    public static BadRequestException SeriesNotPaused(Guid seriesId, string? customMessage = null)
    {
        var exception = new BadRequestException(Area, 16, customMessage ?? "Series is not paused");
        exception.WithDetail("SeriesId", seriesId);
        return exception;
    }

    public static BadRequestException SeriesTimeUpdateNotSupported(string? customMessage = null)
        => new(Area, 17, customMessage ?? "Updating series time is not supported. Please update individual bookings.");

    public static BadRequestException NoBookingsInSeries(Guid seriesId, string? customMessage = null)
    {
        var exception = new BadRequestException(Area, 18, customMessage ?? "No bookings found in series");
        exception.WithDetail("SeriesId", seriesId);
        return exception;
    }

    public static BadRequestException RescheduleLimitExceeded(int rescheduleCount, string? customMessage = null)
    {
        var exception = new BadRequestException(Area, 19, customMessage ?? "This booking has already been rescheduled once");
        exception.WithDetail("RescheduleCount", rescheduleCount);
        return exception;
    }

    public static BadRequestException RescheduleNotAllowedWithin72Hours(DateTime startTime, string? customMessage = null)
    {
        var exception = new BadRequestException(Area, 20, customMessage ?? "Cannot reschedule bookings within 72 hours of the scheduled time");
        exception.WithDetail("BookingStartTime", startTime);
        return exception;
    }

    public static BadRequestException BookingNotConfirmed(BookingStatus status, string? customMessage = null)
    {
        var exception = new BadRequestException(Area, 21, customMessage ?? "Only confirmed bookings can be rescheduled");
        exception.WithDetail("CurrentStatus", status.ToString());
        return exception;
    }

    public static BadRequestException InvalidIdentityVerification(string? customMessage = null)
        => new(Area, 22, customMessage ?? "The email or phone number does not match the booking");

    // Slot Invitation Errors (23-27)
    public static NotFoundException SlotInvitationNotFound(string token, string? customMessage = null)
    {
        var exception = new NotFoundException(Area, 23, customMessage ?? "Slot invitation not found or invalid");
        exception.WithDetail("Token", token);
        return exception;
    }

    public static BadRequestException SlotInvitationEventMismatch(string token, Guid eventId, string? customMessage = null)
    {
        var exception = new BadRequestException(Area, 24, customMessage ?? "This invitation is for a different event");
        exception.WithDetail("Token", token);
        exception.WithDetail("RequestedEventId", eventId);
        return exception;
    }

    public static BadRequestException SlotInvitationNotPending(string token, string? customMessage = null)
    {
        var exception = new BadRequestException(Area, 25, customMessage ?? "This invitation has already been used or is no longer valid");
        exception.WithDetail("Token", token);
        return exception;
    }

    public static BadRequestException SlotInvitationExpired(string token, string? customMessage = null)
    {
        var exception = new BadRequestException(Area, 26, customMessage ?? "This invitation has expired");
        exception.WithDetail("Token", token);
        return exception;
    }
}
