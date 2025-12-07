using System.Text.Json;
using MediatR;
using Meetlr.Application.Common.Interfaces;
using Meetlr.Module.Calendar.Application.Interfaces;
using Meetlr.Module.Calendar.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace Meetlr.Module.Calendar.Application.Commands.DeleteCalendarEvent;

public class DeleteCalendarEventCommandHandler : IRequestHandler<DeleteCalendarEventCommand, DeleteCalendarEventResponse>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IEnumerable<ICalendarProviderService> _calendarProviders;
    private readonly ILogger<DeleteCalendarEventCommandHandler> _logger;

    public DeleteCalendarEventCommandHandler(
        IUnitOfWork unitOfWork,
        IEnumerable<ICalendarProviderService> calendarProviders,
        ILogger<DeleteCalendarEventCommandHandler> logger)
    {
        _unitOfWork = unitOfWork;
        _calendarProviders = calendarProviders;
        _logger = logger;
    }

    public async Task<DeleteCalendarEventResponse> Handle(
        DeleteCalendarEventCommand request,
        CancellationToken cancellationToken)
    {
        var results = new List<CalendarEventDeletionResult>();

        if (string.IsNullOrEmpty(request.CalendarEventId))
        {
            _logger.LogInformation("No calendar event ID provided for deletion");
            return new DeleteCalendarEventResponse { Results = results };
        }

        // Parse the calendar event IDs JSON
        Dictionary<string, string>? eventIds;
        try
        {
            eventIds = JsonSerializer.Deserialize<Dictionary<string, string>>(request.CalendarEventId);
            if (eventIds == null || !eventIds.Any())
            {
                _logger.LogInformation("No event IDs found in CalendarEventId JSON");
                return new DeleteCalendarEventResponse { Results = results };
            }
        }
        catch (JsonException ex)
        {
            _logger.LogError(ex, "Failed to parse CalendarEventId JSON");
            return new DeleteCalendarEventResponse { Results = results };
        }

        _logger.LogInformation("Deleting {EventCount} calendar event(s) for schedule {ScheduleId}", eventIds.Count, request.ScheduleId);

        // Get all active calendar integrations for the schedule
        var integrations = await _unitOfWork.Repository<CalendarIntegration>().GetQueryable()
            .Where(ci => ci.AvailabilityScheduleId == request.ScheduleId && ci.IsActive)
            .ToListAsync(cancellationToken);

        if (!integrations.Any())
        {
            _logger.LogInformation("No active calendar integrations found for schedule {ScheduleId}", request.ScheduleId);
            return new DeleteCalendarEventResponse { Results = results };
        }

        // Delete event from each connected calendar
        foreach (var (providerName, eventId) in eventIds)
        {
            try
            {
                _logger.LogInformation("Processing deletion for provider: {ProviderName}, EventId: {EventId}", providerName, eventId);

                // Find the integration for this provider
                var integration = integrations.FirstOrDefault(i => i.Provider.ToString() == providerName);
                if (integration == null)
                {
                    _logger.LogWarning("No integration found for provider {ProviderName}", providerName);
                    results.Add(new CalendarEventDeletionResult
                    {
                        Provider = providerName,
                        Success = false,
                        Error = "Integration not found"
                    });
                    continue;
                }

                var provider = _calendarProviders.FirstOrDefault(p => p.Provider == integration.Provider);
                if (provider == null)
                {
                    _logger.LogWarning("Provider service not found for {ProviderName}", providerName);
                    results.Add(new CalendarEventDeletionResult
                    {
                        Provider = providerName,
                        Success = false,
                        Error = "Provider service not found"
                    });
                    continue;
                }

                // Check if token is expired and refresh if needed
                var accessToken = integration.AccessToken;
                if (integration.TokenExpiresAt <= DateTime.UtcNow.AddMinutes(5))
                {
                    if (string.IsNullOrEmpty(integration.RefreshToken))
                        continue; // Skip this provider if no refresh token

                    _logger.LogInformation("Token expired, refreshing for {ProviderName}", providerName);
                    var refreshResult = await provider.RefreshAccessTokenAsync(
                        integration.RefreshToken,
                        cancellationToken);

                    accessToken = refreshResult.AccessToken;

                    // Update integration with new token
                    integration.AccessToken = refreshResult.AccessToken;
                    if (!string.IsNullOrEmpty(refreshResult.RefreshToken))
                    {
                        integration.RefreshToken = refreshResult.RefreshToken;
                    }
                    integration.TokenExpiresAt = refreshResult.ExpiresAt;
                    await _unitOfWork.SaveChangesAsync(cancellationToken);
                }

                _logger.LogInformation("Deleting event from {ProviderName} calendar: {EventId}", providerName, eventId);

                // Delete calendar event
                await provider.DeleteCalendarEventAsync(
                    accessToken,
                    eventId,
                    cancellationToken);

                _logger.LogInformation("Successfully deleted event from {ProviderName}", providerName);

                results.Add(new CalendarEventDeletionResult
                {
                    Provider = providerName,
                    Success = true
                });
            }
            catch (Exception ex)
            {
                // Log error but continue with other integrations
                _logger.LogError(ex, "Error deleting event from {ProviderName}", providerName);

                results.Add(new CalendarEventDeletionResult
                {
                    Provider = providerName,
                    Success = false,
                    Error = ex.Message
                });
            }
        }

        _logger.LogInformation("Calendar event deletion completed. Success: {SuccessCount}, Failed: {FailedCount}", results.Count(r => r.Success), results.Count(r => !r.Success));

        return new DeleteCalendarEventResponse
        {
            Success = results.Any(r => r.Success),
            Results = results
        };
    }
}
