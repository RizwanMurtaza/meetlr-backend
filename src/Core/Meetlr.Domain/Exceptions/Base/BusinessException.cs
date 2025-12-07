using Meetlr.Domain.Exceptions.Http;

namespace Meetlr.Domain.Exceptions.Base;

/// <summary>
/// Exception thrown when a business rule is violated
/// </summary>
public class BusinessException : ApplicationExceptionBase
{
    public BusinessException(ApplicationArea area, int messageCode, string message)
        : base(HttpStatusCode.UnprocessableEntity, area, messageCode, message)
    {
    }

    public static BusinessException EventTypeNotActive(Guid eventTypeId)
    {
        return new BusinessException(
            ApplicationArea.EventTypes,
            1,
            "This event type is not available for booking")
            .WithDetail("EventTypeId", eventTypeId) as BusinessException ??
            throw new InvalidOperationException("Failed to create BusinessException");
    }

    public static BusinessException PaymentRequired()
    {
        return new BusinessException(
            ApplicationArea.Bookings,
            1,
            "Payment is required for this booking");
    }

    public static BusinessException MaxBookingsReached()
    {
        return new BusinessException(
            ApplicationArea.Bookings,
            2,
            "Maximum bookings limit has been reached for this time period");
    }

    public static BusinessException OutsideAvailableHours()
    {
        return new BusinessException(
            ApplicationArea.Bookings,
            3,
            "The requested time is outside available hours");
    }

    public static BusinessException InvalidCancellationToken()
    {
        return new BusinessException(
            ApplicationArea.Bookings,
            4,
            "Invalid cancellation token");
    }
}
