using Meetlr.Domain.Exceptions.Base;

namespace Meetlr.Domain.Exceptions.Http;

/// <summary>
/// Exception thrown when a requested resource is not found
/// </summary>
public class NotFoundException : ApplicationExceptionBase
{
    public NotFoundException(ApplicationArea area, int messageCode, string message)
        : base(HttpStatusCode.NotFound, area, messageCode, message)
    {
    }

    public static NotFoundException ForEntity(string entityName, object key)
    {
        return new NotFoundException(
            ApplicationArea.System,
            1,
            $"{entityName} with key '{key}' was not found")
            .WithDetail("EntityName", entityName)
            .WithDetail("Key", key) as NotFoundException ??
            throw new InvalidOperationException("Failed to create NotFoundException");
    }

    public static NotFoundException ForUser(Guid userId)
    {
        return new NotFoundException(
            ApplicationArea.Users,
            1,
            $"User with ID '{userId}' was not found")
            .WithDetail("UserId", userId) as NotFoundException ??
            throw new InvalidOperationException("Failed to create NotFoundException");
    }

    public static NotFoundException ForEventType(Guid eventTypeId)
    {
        return new NotFoundException(
            ApplicationArea.EventTypes,
            1,
            $"Event type with ID '{eventTypeId}' was not found")
            .WithDetail("EventTypeId", eventTypeId) as NotFoundException ??
            throw new InvalidOperationException("Failed to create NotFoundException");
    }

    public static NotFoundException ForBooking(Guid bookingId)
    {
        return new NotFoundException(
            ApplicationArea.Bookings,
            1,
            $"Booking with ID '{bookingId}' was not found")
            .WithDetail("BookingId", bookingId) as NotFoundException ??
            throw new InvalidOperationException("Failed to create NotFoundException");
    }

    public static NotFoundException ForTenant(string subdomain)
    {
        return new NotFoundException(
            ApplicationArea.Tenants,
            1,
            $"Tenant with subdomain '{subdomain}' was not found")
            .WithDetail("Subdomain", subdomain) as NotFoundException ??
            throw new InvalidOperationException("Failed to create NotFoundException");
    }
}
