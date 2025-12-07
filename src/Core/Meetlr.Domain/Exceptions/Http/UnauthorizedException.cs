using Meetlr.Domain.Exceptions.Base;

namespace Meetlr.Domain.Exceptions.Http;

/// <summary>
/// Exception thrown when authentication is required but not provided
/// </summary>
public class UnauthorizedException : ApplicationExceptionBase
{
    public UnauthorizedException(ApplicationArea area, int messageCode, string message)
        : base(HttpStatusCode.Unauthorized, area, messageCode, message)
    {
    }

    public static UnauthorizedException NotAuthenticated()
    {
        return new UnauthorizedException(
            ApplicationArea.Authentication,
            1,
            "User is not authenticated");
    }

    public static UnauthorizedException InvalidCredentials()
    {
        return new UnauthorizedException(
            ApplicationArea.Authentication,
            2,
            "Invalid email or password");
    }

    public static UnauthorizedException TokenExpired()
    {
        return new UnauthorizedException(
            ApplicationArea.Authentication,
            3,
            "Authentication token has expired");
    }

    public static UnauthorizedException InvalidToken()
    {
        return new UnauthorizedException(
            ApplicationArea.Authentication,
            4,
            "Invalid authentication token");
    }
}
