using Meetlr.Domain.Exceptions.Base;

namespace Meetlr.Domain.Exceptions.Http;

/// <summary>
/// Exception thrown when a user doesn't have permission to perform an action
/// </summary>
public class ForbiddenException : ApplicationExceptionBase
{
    public ForbiddenException(ApplicationArea area, int messageCode, string message)
        : base(HttpStatusCode.Forbidden, area, messageCode, message)
    {
    }

    public static ForbiddenException InsufficientPermissions()
    {
        return new ForbiddenException(
            ApplicationArea.Authentication,
            1,
            "You do not have permission to perform this action");
    }

    public static ForbiddenException NotResourceOwner(string resourceType)
    {
        return new ForbiddenException(
            ApplicationArea.Authentication,
            2,
            $"You do not have permission to access this {resourceType}")
            .WithDetail("ResourceType", resourceType) as ForbiddenException ??
            throw new InvalidOperationException("Failed to create ForbiddenException");
    }

    public static ForbiddenException NotTenantMember()
    {
        return new ForbiddenException(
            ApplicationArea.Tenants,
            1,
            "You do not belong to this tenant");
    }

    public static ForbiddenException NotAdminUser()
    {
        return new ForbiddenException(
            ApplicationArea.Authentication,
            3,
            "This action requires administrator privileges");
    }
}
