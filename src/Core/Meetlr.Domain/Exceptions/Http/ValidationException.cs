using Meetlr.Domain.Exceptions.Base;

namespace Meetlr.Domain.Exceptions.Http;

/// <summary>
/// Exception thrown when validation fails
/// </summary>
public class ValidationException : ApplicationExceptionBase
{
    public ValidationException(ApplicationArea area, int messageCode, string message)
        : base(HttpStatusCode.BadRequest, area, messageCode, message)
    {
    }

    public static ValidationException InvalidInput(string fieldName, string reason)
    {
        return new ValidationException(
            ApplicationArea.System,
            1,
            $"Invalid input for {fieldName}: {reason}")
            .WithDetail("FieldName", fieldName)
            .WithDetail("Reason", reason) as ValidationException ??
            throw new InvalidOperationException("Failed to create ValidationException");
    }

    public static ValidationException RequiredField(string fieldName)
    {
        return new ValidationException(
            ApplicationArea.System,
            2,
            $"{fieldName} is required")
            .WithDetail("FieldName", fieldName) as ValidationException ??
            throw new InvalidOperationException("Failed to create ValidationException");
    }

    public static ValidationException InvalidTimeSlot(string reason)
    {
        return new ValidationException(
            ApplicationArea.Bookings,
            1,
            reason);
    }

    public static ValidationException InvalidDateRange(DateTime start, DateTime end)
    {
        return new ValidationException(
            ApplicationArea.System,
            3,
            "End date must be after start date")
            .WithDetail("StartDate", start)
            .WithDetail("EndDate", end) as ValidationException ??
            throw new InvalidOperationException("Failed to create ValidationException");
    }

    public static ValidationException PastBookingNotAllowed()
    {
        return new ValidationException(
            ApplicationArea.Bookings,
            2,
            "Cannot book a time slot in the past");
    }

    public static ValidationException MinimumNoticeRequired(int minutes)
    {
        return new ValidationException(
            ApplicationArea.Bookings,
            3,
            $"This event requires at least {minutes} minutes advance notice")
            .WithDetail("RequiredMinutes", minutes) as ValidationException ??
            throw new InvalidOperationException("Failed to create ValidationException");
    }
}
