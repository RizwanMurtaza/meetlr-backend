using Meetlr.Application.Common.Interfaces;
using Meetlr.Application.Interfaces;
using Meetlr.Application.Interfaces.Models;
using Meetlr.Module.Calendar.Application.Interfaces;
using Meetlr.Module.Calendar.Application.Models;
using Meetlr.Module.Calendar.Domain.Entities;
using Meetlr.Module.Calendar.Domain.Enums;
using Microsoft.EntityFrameworkCore;

namespace Meetlr.Module.Calendar.Infrastructure.Services;

/// <summary>
/// Implementation of ICalendarService that aggregates calendar operations across all connected calendar providers.
/// Calendar integrations are now linked at the schedule level (AvailabilityScheduleId).
/// Handles token management and delegates to ICalendarProviderService implementations.
/// </summary>
internal class CalendarService : ICalendarService
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IEnumerable<ICalendarProviderService> _providers;

    public CalendarService(
        IUnitOfWork unitOfWork,
        IEnumerable<ICalendarProviderService> providers)
    {
        _unitOfWork = unitOfWork;
        _providers = providers;
    }

    public async Task<List<CalendarBusySlot>> GetBusyTimesAsync(
        Guid scheduleId,
        DateTime startDate,
        DateTime endDate,
        CancellationToken cancellationToken = default)
    {
        var busySlots = new List<CalendarBusySlot>();

        // Get all active calendar integrations for this schedule
        var integrations = await _unitOfWork.Repository<CalendarIntegration>()
            .GetQueryable()
            .Where(ci => ci.AvailabilityScheduleId == scheduleId && ci.IsActive && !ci.IsDeleted)
            .ToListAsync(cancellationToken);

        foreach (var integration in integrations)
        {
            try
            {
                var provider = _providers.FirstOrDefault(p => p.Provider == integration.Provider);
                if (provider == null)
                    continue;

                var accessToken = await EnsureValidTokenAsync(integration, provider, cancellationToken);
                var slots = await provider.GetBusyTimesAsync(accessToken, startDate, endDate, cancellationToken);

                foreach (var slot in slots)
                {
                    busySlots.Add(new CalendarBusySlot
                    {
                        StartTime = slot.StartTime,
                        EndTime = slot.EndTime,
                        Source = provider.ProviderName
                    });
                }
            }
            catch
            {
                // Continue with other integrations if one fails
            }
        }

        return busySlots;
    }

    public async Task<CalendarServiceEventResult> CreateEventAsync(
        Guid scheduleId,
        CalendarServiceEventRequest request,
        CancellationToken cancellationToken = default)
    {
        // Get primary or first active calendar integration for this schedule
        var integration = await _unitOfWork.Repository<CalendarIntegration>()
            .GetQueryable()
            .Where(ci => ci.AvailabilityScheduleId == scheduleId && ci.IsActive && !ci.IsDeleted)
            .OrderByDescending(ci => ci.IsPrimaryCalendar)
            .FirstOrDefaultAsync(cancellationToken);

        if (integration == null)
        {
            return CalendarServiceEventResult.Failed("No connected calendar found for this schedule");
        }

        var provider = _providers.FirstOrDefault(p => p.Provider == integration.Provider);
        if (provider == null)
        {
            return CalendarServiceEventResult.Failed($"No provider found for {integration.Provider}");
        }

        try
        {
            var accessToken = await EnsureValidTokenAsync(integration, provider, cancellationToken);

            var eventDetails = new CalendarEventDetails
            {
                Summary = request.Summary,
                Description = request.Description,
                StartTime = request.StartTime,
                EndTime = request.EndTime,
                TimeZone = request.TimeZone,
                Location = request.Location,
                MeetingLink = request.MeetingLink,
                AttendeeEmails = request.AttendeeEmails
            };

            var eventId = await provider.CreateCalendarEventAsync(accessToken, eventDetails, cancellationToken);

            return new CalendarServiceEventResult
            {
                Success = true,
                EventId = eventId
            };
        }
        catch (Exception ex)
        {
            return CalendarServiceEventResult.Failed(ex.Message);
        }
    }

    public async Task<bool> DeleteEventAsync(
        Guid scheduleId,
        string calendarEventId,
        CancellationToken cancellationToken = default)
    {
        // Get all active calendar integrations for this schedule
        var integrations = await _unitOfWork.Repository<CalendarIntegration>()
            .GetQueryable()
            .Where(ci => ci.AvailabilityScheduleId == scheduleId && ci.IsActive && !ci.IsDeleted)
            .ToListAsync(cancellationToken);

        foreach (var integration in integrations)
        {
            try
            {
                var provider = _providers.FirstOrDefault(p => p.Provider == integration.Provider);
                if (provider == null)
                    continue;

                var accessToken = await EnsureValidTokenAsync(integration, provider, cancellationToken);
                await provider.DeleteCalendarEventAsync(accessToken, calendarEventId, cancellationToken);
                return true;
            }
            catch
            {
                // Try next integration
            }
        }

        return false;
    }

    public async Task<CalendarServiceSeriesResult> CreateSeriesEventsAsync(
        Guid scheduleId,
        List<CalendarServiceEventRequest> events,
        CancellationToken cancellationToken = default)
    {
        var results = new List<CalendarServiceEventResult>();
        var successCount = 0;
        var failureCount = 0;

        foreach (var eventRequest in events)
        {
            var result = await CreateEventAsync(scheduleId, eventRequest, cancellationToken);
            results.Add(result);

            if (result.Success)
                successCount++;
            else
                failureCount++;
        }

        return new CalendarServiceSeriesResult
        {
            Success = failureCount == 0,
            SuccessCount = successCount,
            FailureCount = failureCount,
            Results = results
        };
    }

    public async Task<bool> HasConnectedCalendarAsync(
        Guid scheduleId,
        CancellationToken cancellationToken = default)
    {
        return await _unitOfWork.Repository<CalendarIntegration>()
            .GetQueryable()
            .AnyAsync(ci => ci.AvailabilityScheduleId == scheduleId && ci.IsActive && !ci.IsDeleted, cancellationToken);
    }

    public async Task<List<CalendarIntegrationDto>> GetCalendarsForScheduleAsync(
        Guid scheduleId,
        CancellationToken cancellationToken = default)
    {
        var integrations = await _unitOfWork.Repository<CalendarIntegration>()
            .GetQueryable()
            .Where(ci => ci.AvailabilityScheduleId == scheduleId && !ci.IsDeleted)
            .Select(ci => new CalendarIntegrationDto
            {
                Id = ci.Id,
                AvailabilityScheduleId = ci.AvailabilityScheduleId,
                Provider = ci.Provider.ToString(),
                Email = ci.ProviderEmail,
                IsActive = ci.IsActive,
                IsPrimaryCalendar = ci.IsPrimaryCalendar,
                CheckForConflicts = ci.CheckForConflicts,
                AddEventsToCalendar = ci.AddEventsToCalendar,
                LastSyncedAt = ci.LastSyncedAt
            })
            .ToListAsync(cancellationToken);

        return integrations;
    }

    public async Task<bool> DisconnectCalendarAsync(
        Guid calendarIntegrationId,
        CancellationToken cancellationToken = default)
    {
        try
        {
            var integration = await _unitOfWork.Repository<CalendarIntegration>()
                .GetQueryable()
                .FirstOrDefaultAsync(ci => ci.Id == calendarIntegrationId && !ci.IsDeleted, cancellationToken);

            if (integration == null)
                return false;

            integration.IsActive = false;
            integration.IsDeleted = true;
            integration.DeletedAt = DateTime.UtcNow;

            await _unitOfWork.SaveChangesAsync(cancellationToken);
            return true;
        }
        catch
        {
            return false;
        }
    }

    public async Task<bool> ConnectCalendarToScheduleAsync(
        Guid scheduleId,
        string provider,
        string email,
        string accessToken,
        string? refreshToken,
        DateTime? tokenExpiry,
        CancellationToken cancellationToken = default)
    {
        try
        {
            var calendarProvider = provider.ToLowerInvariant() switch
            {
                "google" => CalendarProvider.Google,
                "outlook" => CalendarProvider.Outlook,
                "office365" => CalendarProvider.Office365,
                _ => throw new ArgumentException($"Unknown calendar provider: {provider}")
            };

            // Check if integration already exists for this schedule and provider
            var existingIntegration = await _unitOfWork.Repository<CalendarIntegration>()
                .GetQueryable()
                .FirstOrDefaultAsync(
                    ci => ci.AvailabilityScheduleId == scheduleId &&
                          ci.Provider == calendarProvider &&
                          !ci.IsDeleted,
                    cancellationToken);

            if (existingIntegration != null)
            {
                // Update existing integration
                existingIntegration.AccessToken = accessToken;
                existingIntegration.RefreshToken = refreshToken;
                existingIntegration.TokenExpiresAt = tokenExpiry;
                existingIntegration.ProviderEmail = email;
                existingIntegration.IsActive = true;
            }
            else
            {
                // Check if this is the first integration for the schedule (make it primary)
                var hasExistingIntegrations = await _unitOfWork.Repository<CalendarIntegration>()
                    .GetQueryable()
                    .AnyAsync(ci => ci.AvailabilityScheduleId == scheduleId && !ci.IsDeleted, cancellationToken);

                // Create new integration
                var integration = new CalendarIntegration
                {
                    AvailabilityScheduleId = scheduleId,
                    Provider = calendarProvider,
                    AccessToken = accessToken,
                    RefreshToken = refreshToken,
                    TokenExpiresAt = tokenExpiry,
                    ProviderEmail = email,
                    IsActive = true,
                    IsPrimaryCalendar = !hasExistingIntegrations, // First calendar is primary
                    CheckForConflicts = true,
                    AddEventsToCalendar = true
                };

                _unitOfWork.Repository<CalendarIntegration>().Add(integration);
            }

            await _unitOfWork.SaveChangesAsync(cancellationToken);
            return true;
        }
        catch
        {
            return false;
        }
    }

    private async Task<string> EnsureValidTokenAsync(
        CalendarIntegration integration,
        ICalendarProviderService provider,
        CancellationToken cancellationToken)
    {
        // Check if token is expired or about to expire (within 5 minutes)
        if (integration.TokenExpiresAt.HasValue &&
            integration.TokenExpiresAt.Value <= DateTime.UtcNow.AddMinutes(5))
        {
            if (string.IsNullOrEmpty(integration.RefreshToken))
            {
                throw new InvalidOperationException($"{provider.ProviderName} token expired and no refresh token available. Please reconnect your calendar.");
            }

            // Refresh the token
            var tokenResult = await provider.RefreshAccessTokenAsync(integration.RefreshToken, cancellationToken);

            integration.AccessToken = tokenResult.AccessToken;
            if (!string.IsNullOrEmpty(tokenResult.RefreshToken))
            {
                integration.RefreshToken = tokenResult.RefreshToken;
            }
            integration.TokenExpiresAt = tokenResult.ExpiresAt;

            await _unitOfWork.SaveChangesAsync(cancellationToken);
        }

        return integration.AccessToken;
    }
}
