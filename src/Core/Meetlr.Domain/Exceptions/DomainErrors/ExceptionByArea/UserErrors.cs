using Meetlr.Domain.Exceptions.Base;
using Meetlr.Domain.Exceptions.Http;

namespace Meetlr.Domain.Exceptions.DomainErrors.ExceptionByArea;

/// <summary>
/// Defines errors related to user management.
/// All errors use ApplicationArea.Users (area code: 2)
/// </summary>
public static class UserErrors
{
    private const ApplicationArea Area = ApplicationArea.Users;

    public static NotFoundException UserNotFound(Guid userId, string? customMessage = null)
    {
        var exception = new NotFoundException(Area, 1, customMessage ?? "User not found");
        exception.WithDetail("UserId", userId);
        return exception;
    }

    public static ConflictException UserAlreadyExists(string email, string? customMessage = null)
    {
        var exception = new ConflictException(Area, 2, customMessage ?? "A user with this email already exists");
        exception.WithDetail("Email", email);
        return exception;
    }

    public static ForbiddenException UserInactive(string? customMessage = null)
        => new(Area, 3, customMessage ?? "User account is inactive");

    public static BadRequestException UpdateProfileFailed(string errors, string? customMessage = null)
    {
        var exception = new BadRequestException(Area, 4, customMessage ?? $"Failed to update profile: {errors}");
        exception.WithDetail("Errors", errors);
        return exception;
    }

    public static ForbiddenException NotTenantMember(string? customMessage = null)
        => new(Area, 5, customMessage ?? "You do not belong to this tenant");

    public static ConflictException UsernameAlreadyTaken(string username, string? customMessage = null)
    {
        var exception = new ConflictException(Area, 6, customMessage ?? $"Username '{username}' is already taken");
        exception.WithDetail("Username", username);
        return exception;
    }

    public static NotFoundException UserNotFoundByEmail(string email, string? customMessage = null)
    {
        var exception = new NotFoundException(Area, 7, customMessage ?? "User not found");
        exception.WithDetail("Email", email);
        return exception;
    }
}
