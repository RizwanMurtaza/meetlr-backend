using Meetlr.Domain.Exceptions.Base;
using Meetlr.Domain.Exceptions.Http;

namespace Meetlr.Module.Calendar.Domain.Errors;

/// <summary>
/// Defines errors related to calendar integrations.
/// All errors use ApplicationArea.CalendarIntegration (area code: 8)
/// </summary>
public static class CalendarErrors
{
    private const ApplicationArea Area = ApplicationArea.CalendarIntegration;

    public static NotFoundException CalendarIntegrationNotFound(Guid integrationId, string? customMessage = null)
    {
        var exception = new NotFoundException(Area, 1, customMessage ?? "Calendar integration not found");
        exception.WithDetail("IntegrationId", integrationId);
        return exception;
    }

    public static BadRequestException RefreshTokenNotAvailable(string? customMessage = null)
    {
        var exception = new BadRequestException(Area, 2, customMessage ?? "Refresh token is not available for this calendar integration");
        exception.WithDetail("ErrorCode", "REFRESH_TOKEN_NOT_AVAILABLE");
        return exception;
    }

    public static NotFoundException CalendarProviderNotFound(string provider, string? customMessage = null)
    {
        var exception = new NotFoundException(Area, 3, customMessage ?? "Calendar provider not found");
        exception.WithDetail("Provider", provider);
        return exception;
    }

    public static BadRequestException TokenRefreshFailed(string provider, string? customMessage = null)
    {
        var exception = new BadRequestException(Area, 4, customMessage ?? "Failed to refresh calendar token");
        exception.WithDetail("Provider", provider);
        return exception;
    }

    public static BadRequestException CalendarEventCreationFailed(string provider, string? customMessage = null)
    {
        var exception = new BadRequestException(Area, 5, customMessage ?? "Failed to create calendar event");
        exception.WithDetail("Provider", provider);
        return exception;
    }
}
