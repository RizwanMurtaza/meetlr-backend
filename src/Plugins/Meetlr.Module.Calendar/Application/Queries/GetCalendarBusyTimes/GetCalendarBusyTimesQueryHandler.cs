using MediatR;
using Meetlr.Application.Common.Interfaces;
using Meetlr.Module.Calendar.Application.Commands.RefreshCalendarToken;
using Meetlr.Module.Calendar.Application.Interfaces;
using Meetlr.Module.Calendar.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace Meetlr.Module.Calendar.Application.Queries.GetCalendarBusyTimes;

public class GetCalendarBusyTimesQueryHandler : IRequestHandler<GetCalendarBusyTimesQuery, GetCalendarBusyTimesResponse>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IEnumerable<ICalendarProviderService> _calendarProviders;
    private readonly IMediator _mediator;
    private readonly ILogger<GetCalendarBusyTimesQueryHandler> _logger;

    public GetCalendarBusyTimesQueryHandler(
        IUnitOfWork unitOfWork,
        IEnumerable<ICalendarProviderService> calendarProviders,
        IMediator mediator,
        ILogger<GetCalendarBusyTimesQueryHandler> logger)
    {
        _unitOfWork = unitOfWork;
        _calendarProviders = calendarProviders;
        _mediator = mediator;
        _logger = logger;
    }

    public async Task<GetCalendarBusyTimesResponse> Handle(
        GetCalendarBusyTimesQuery request,
        CancellationToken cancellationToken)
    {
        var busyTimes = new List<BusyTimeSlotDto>();

        // Get all active calendar integrations for the schedule
        var integrations = await _unitOfWork.Repository<CalendarIntegration>().GetQueryable()
            .Where(ci => ci.AvailabilityScheduleId == request.ScheduleId && ci.IsActive)
            .ToListAsync(cancellationToken);

        if (!integrations.Any())
        {
            return new GetCalendarBusyTimesResponse { BusyTimes = busyTimes };
        }

        // Fetch busy times from each connected calendar
        foreach (var integration in integrations)
        {
            try
            {
                var provider = _calendarProviders.FirstOrDefault(p => p.Provider == integration.Provider);
                if (provider == null)
                {
                    _logger.LogWarning("Calendar provider {Provider} not found for integration {IntegrationId}",
                        integration.Provider, integration.Id);
                    continue;
                }

                // Check if token is expired and refresh if needed
                var accessToken = integration.AccessToken;
                if (integration.TokenExpiresAt <= DateTime.UtcNow.AddMinutes(5))
                {
                    if (string.IsNullOrEmpty(integration.RefreshToken))
                    {
                        _logger.LogWarning("Calendar integration {IntegrationId} has expired token but no refresh token available", integration.Id);
                        continue; // Skip this integration if no refresh token
                    }

                    // Use command to refresh token (maintains CQRS - queries should not modify state)
                    var refreshCommand = new RefreshCalendarTokenCommand
                    {
                        IntegrationId = integration.Id
                    };

                    var refreshResult = await _mediator.Send(refreshCommand, cancellationToken);
                    accessToken = refreshResult.AccessToken;

                    _logger.LogInformation("Successfully refreshed calendar token for integration {IntegrationId}", integration.Id);
                }

                // Fetch busy times from the calendar
                var providerBusyTimes = await provider.GetBusyTimesAsync(
                    accessToken,
                    request.StartDate,
                    request.EndDate,
                    cancellationToken);

                // Map to DTOs
                busyTimes.AddRange(providerBusyTimes.Select(bt => new BusyTimeSlotDto
                {
                    StartTime = bt.StartTime,
                    EndTime = bt.EndTime,
                    Summary = bt.Summary,
                    Provider = integration.Provider.ToString()
                }));
            }
            catch (Exception ex)
            {
                // Log error but continue with other integrations
                _logger.LogError(ex, "Error fetching busy times from {Provider} for integration {IntegrationId}",
                    integration.Provider, integration.Id);
            }
        }

        return new GetCalendarBusyTimesResponse
        {
            BusyTimes = busyTimes.OrderBy(bt => bt.StartTime).ToList()
        };
    }
}
