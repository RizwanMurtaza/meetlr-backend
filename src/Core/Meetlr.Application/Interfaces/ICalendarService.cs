using Meetlr.Application.Interfaces.Models;

namespace Meetlr.Application.Interfaces;

/// <summary>
/// Service interface for calendar operations.
/// Implemented by calendar plugins to provide calendar functionality.
/// Calendar integrations are now linked at the schedule level (not user level).
/// </summary>
public interface ICalendarService
{
    /// <summary>
    /// Get busy time slots from connected calendars for a schedule
    /// </summary>
    Task<List<CalendarBusySlot>> GetBusyTimesAsync(
        Guid scheduleId,
        DateTime startDate,
        DateTime endDate,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Create a calendar event for a schedule
    /// </summary>
    Task<CalendarServiceEventResult> CreateEventAsync(
        Guid scheduleId,
        CalendarServiceEventRequest request,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Delete a calendar event
    /// </summary>
    Task<bool> DeleteEventAsync(
        Guid scheduleId,
        string calendarEventId,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Create calendar events for a series of bookings
    /// </summary>
    Task<CalendarServiceSeriesResult> CreateSeriesEventsAsync(
        Guid scheduleId,
        List<CalendarServiceEventRequest> events,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Check if schedule has any calendar connected
    /// </summary>
    Task<bool> HasConnectedCalendarAsync(
        Guid scheduleId,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Connect or update a calendar integration to a schedule from OAuth tokens
    /// </summary>
    Task<bool> ConnectCalendarToScheduleAsync(
        Guid scheduleId,
        string provider,
        string email,
        string accessToken,
        string? refreshToken,
        DateTime? tokenExpiry,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Get all calendar integrations for a schedule
    /// </summary>
    Task<List<CalendarIntegrationDto>> GetCalendarsForScheduleAsync(
        Guid scheduleId,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Disconnect a calendar integration
    /// </summary>
    Task<bool> DisconnectCalendarAsync(
        Guid calendarIntegrationId,
        CancellationToken cancellationToken = default);
}
