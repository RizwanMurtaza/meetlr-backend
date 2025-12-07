using Meetlr.Domain.Exceptions.Base;
using Meetlr.Domain.Exceptions.Http;

namespace Meetlr.Domain.Exceptions.DomainErrors.ExceptionByArea;

/// <summary>
/// Defines errors related to authentication and authorization.
/// All errors use ApplicationArea.Authentication (area code: 3)
/// </summary>
public static class AuthenticationErrors
{
    private const ApplicationArea Area = ApplicationArea.Authentication;

    public static UnauthorizedException UserNotAuthenticated(string? customMessage = null)
        => new(Area, 1, customMessage ?? "User is not authenticated");

    public static UnauthorizedException InvalidUserId(string? customMessage = null)
        => new(Area, 2, customMessage ?? "User ID is not in valid format");

    public static UnauthorizedException InvalidCredentials(string? customMessage = null)
        => new(Area, 3, customMessage ?? "Invalid email or password");

    public static ForbiddenException InsufficientPermissions(string? customMessage = null)
        => new(Area, 4, customMessage ?? "User does not have sufficient permissions");

    public static UnauthorizedException TokenExpired(string? customMessage = null)
        => new(Area, 5, customMessage ?? "Authentication token has expired");

    public static UnauthorizedException InvalidToken(string? customMessage = null)
        => new(Area, 6, customMessage ?? "Authentication token is invalid");

    public static ForbiddenException NotAdminUser(string? customMessage = null)
        => new(Area, 7, customMessage ?? "This action requires administrator privileges");

    public static UnauthorizedException EmailNotConfirmed(string? customMessage = null)
        => new(Area, 8, customMessage ?? "Email address has not been confirmed. Please check your email for the verification code.");

    public static UnauthorizedException InvalidRefreshToken(string? customMessage = null)
        => new(Area, 9, customMessage ?? "Refresh token is invalid or has expired");

    public static BadRequestException TooManyRequests(int secondsRemaining, string? customMessage = null)
    {
        var exception = new BadRequestException(Area, 10, customMessage ?? $"Please wait {secondsRemaining} seconds before trying again");
        exception.WithDetail("SecondsRemaining", secondsRemaining);
        return exception;
    }

    public static BadRequestException VerificationCodeExpired(string? customMessage = null)
        => new(Area, 11, customMessage ?? "The verification code has expired. Please request a new one.");

    public static BadRequestException InvalidVerificationCode(string? customMessage = null)
        => new(Area, 12, customMessage ?? "The verification code is invalid");

    public static BadRequestException PasswordResetFailed(string errors, string? customMessage = null)
    {
        var exception = new BadRequestException(Area, 13, customMessage ?? $"Password reset failed: {errors}");
        exception.WithDetail("Errors", errors);
        return exception;
    }

    public static BadRequestException RegistrationFailed(string errors, string? customMessage = null)
    {
        var exception = new BadRequestException(Area, 14, customMessage ?? $"Registration failed: {errors}");
        exception.WithDetail("Errors", errors);
        return exception;
    }
}
