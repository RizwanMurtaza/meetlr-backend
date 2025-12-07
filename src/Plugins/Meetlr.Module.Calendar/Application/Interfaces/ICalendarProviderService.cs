using Meetlr.Module.Calendar.Application.Models;
using Meetlr.Module.Calendar.Domain.Enums;

namespace Meetlr.Module.Calendar.Application.Interfaces;

/// <summary>
/// Low-level interface for calendar provider HTTP operations (Google, Microsoft, etc.)
/// Used by CQRS handlers to interact with external calendar APIs
/// </summary>
public interface ICalendarProviderService
{
    /// <summary>
    /// Gets the provider type this service handles
    /// </summary>
    CalendarProvider Provider { get; }

    /// <summary>
    /// Gets the display name of the provider (e.g., "Google Calendar", "Outlook Calendar")
    /// </summary>
    string ProviderName { get; }

    /// <summary>
    /// Fetches busy time slots from the provider's calendar
    /// </summary>
    Task<List<BusyTimeSlot>> GetBusyTimesAsync(
        string accessToken,
        DateTime startDate,
        DateTime endDate,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Creates a calendar event in the provider's calendar
    /// </summary>
    Task<string> CreateCalendarEventAsync(
        string accessToken,
        CalendarEventDetails eventDetails,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Updates an existing calendar event
    /// </summary>
    Task UpdateCalendarEventAsync(
        string accessToken,
        string eventId,
        CalendarEventDetails eventDetails,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Deletes a calendar event
    /// </summary>
    Task DeleteCalendarEventAsync(
        string accessToken,
        string eventId,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Refreshes an expired access token
    /// </summary>
    Task<TokenRefreshResult> RefreshAccessTokenAsync(
        string refreshToken,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets the OAuth authorization URL for the provider
    /// </summary>
    string GetAuthorizationUrl(string redirectUri, string state);

    /// <summary>
    /// Exchanges an OAuth authorization code for access/refresh tokens
    /// </summary>
    Task<TokenRefreshResult> ExchangeCodeForTokenAsync(string code, string redirectUri);

    /// <summary>
    /// Gets the user's email address from the provider
    /// </summary>
    Task<string?> GetUserEmailAsync(string accessToken);
}
