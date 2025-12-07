using Meetlr.Domain.Exceptions.Base;

namespace Meetlr.Domain.Exceptions.Http;

/// <summary>
/// Exception thrown when there's a conflict with existing data
/// </summary>
public class ConflictException : ApplicationExceptionBase
{
    public ConflictException(ApplicationArea area, int messageCode, string message)
        : base(HttpStatusCode.Conflict, area, messageCode, message)
    {
    }

    public static ConflictException EmailAlreadyExists(string email)
    {
        return new ConflictException(
            ApplicationArea.Users,
            1,
            "A user with this email already exists")
            .WithDetail("Email", email) as ConflictException ??
            throw new InvalidOperationException("Failed to create ConflictException");
    }

    public static ConflictException SubdomainAlreadyExists(string subdomain)
    {
        return new ConflictException(
            ApplicationArea.Tenants,
            1,
            "This subdomain is already taken")
            .WithDetail("Subdomain", subdomain) as ConflictException ??
            throw new InvalidOperationException("Failed to create ConflictException");
    }

    public static ConflictException SlugAlreadyExists(string slug)
    {
        return new ConflictException(
            ApplicationArea.EventTypes,
            1,
            "An event type with this slug already exists")
            .WithDetail("Slug", slug) as ConflictException ??
            throw new InvalidOperationException("Failed to create ConflictException");
    }

    public static ConflictException TimeSlotConflict(DateTime startTime, DateTime endTime)
    {
        return new ConflictException(
            ApplicationArea.Bookings,
            1,
            "This time slot is no longer available")
            .WithDetail("StartTime", startTime)
            .WithDetail("EndTime", endTime) as ConflictException ??
            throw new InvalidOperationException("Failed to create ConflictException");
    }

    public static ConflictException BookingAlreadyCancelled()
    {
        return new ConflictException(
            ApplicationArea.Bookings,
            2,
            "This booking has already been cancelled");
    }
}
