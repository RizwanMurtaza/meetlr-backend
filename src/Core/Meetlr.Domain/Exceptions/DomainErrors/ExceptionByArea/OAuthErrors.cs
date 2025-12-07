using Meetlr.Domain.Exceptions.Base;
using Meetlr.Domain.Exceptions.Http;

namespace Meetlr.Domain.Exceptions.DomainErrors.ExceptionByArea;

/// <summary>
/// Defines errors related to OAuth and external provider integrations.
/// All errors use ApplicationArea.OAuth (area code: 16)
/// </summary>
public static class OAuthErrors
{
    private const ApplicationArea Area = ApplicationArea.OAuth;

    public static BadRequestException ProviderNotSupported(string provider, string? customMessage = null)
    {
        var exception = new BadRequestException(Area, 1, customMessage ?? $"OAuth provider '{provider}' is not supported");
        exception.WithDetail("Provider", provider);
        return exception;
    }

    public static BadRequestException TokenExchangeFailed(string provider, string? customMessage = null)
    {
        var exception = new BadRequestException(Area, 2, customMessage ?? $"Failed to exchange authorization code for token");
        exception.WithDetail("Provider", provider);
        return exception;
    }

    public static BadRequestException TokenRefreshFailed(string provider, string? customMessage = null)
    {
        var exception = new BadRequestException(Area, 3, customMessage ?? $"Failed to refresh token");
        exception.WithDetail("Provider", provider);
        return exception;
    }

    public static BadRequestException UserInfoParseFailed(string provider, string? customMessage = null)
    {
        var exception = new BadRequestException(Area, 4, customMessage ?? $"Failed to parse user info from {provider}");
        exception.WithDetail("Provider", provider);
        return exception;
    }

    public static BadRequestException InvalidAuthorizationCode(string? customMessage = null)
        => new(Area, 5, customMessage ?? "Invalid or expired authorization code");

    public static BadRequestException MissingRequiredScope(string scope, string? customMessage = null)
    {
        var exception = new BadRequestException(Area, 6, customMessage ?? $"Required OAuth scope '{scope}' was not granted");
        exception.WithDetail("Scope", scope);
        return exception;
    }
}
