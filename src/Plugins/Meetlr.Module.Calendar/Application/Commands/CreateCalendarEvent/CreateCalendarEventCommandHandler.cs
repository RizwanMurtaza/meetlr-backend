using MediatR;
using Meetlr.Application.Common.Interfaces;
using Meetlr.Module.Calendar.Application.Interfaces;
using Meetlr.Module.Calendar.Application.Models;
using Meetlr.Module.Calendar.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace Meetlr.Module.Calendar.Application.Commands.CreateCalendarEvent;

public class CreateCalendarEventCommandHandler : IRequestHandler<CreateCalendarEventCommand, CreateCalendarEventResponse>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IEnumerable<ICalendarProviderService> _calendarProviders;
    private readonly ILogger<CreateCalendarEventCommandHandler> _logger;

    public CreateCalendarEventCommandHandler(
        IUnitOfWork unitOfWork,
        IEnumerable<ICalendarProviderService> calendarProviders,
        ILogger<CreateCalendarEventCommandHandler> logger)
    {
        _unitOfWork = unitOfWork;
        _calendarProviders = calendarProviders;
        _logger = logger;
    }

    public async Task<CreateCalendarEventResponse> Handle(
        CreateCalendarEventCommand request,
        CancellationToken cancellationToken)
    {
        var results = new List<CalendarEventCreationResult>();

        var integration = await _unitOfWork.Repository<CalendarIntegration>().GetQueryable()
            .Where(ci => ci.AvailabilityScheduleId == request.ScheduleId && ci.IsActive && ci.IsPrimaryCalendar && ci.AddEventsToCalendar)
            .FirstOrDefaultAsync(cancellationToken);

        if (integration == null)
        {
            _logger.LogInformation("No primary calendar configured to receive events for schedule {ScheduleId}", request.ScheduleId);
            return new CreateCalendarEventResponse { Results = results };
        }

        try
        {
            var provider = _calendarProviders.FirstOrDefault(p => p.Provider == integration.Provider);
            if (provider == null)
            {
                results.Add(new CalendarEventCreationResult
                {
                    Provider = integration.Provider.ToString(),
                    Success = false,
                    Error = "Provider service not found"
                });
                return new CreateCalendarEventResponse { Results = results };
            }

            var accessToken = integration.AccessToken;
            if (integration.TokenExpiresAt <= DateTime.UtcNow.AddMinutes(5))
            {
                if (string.IsNullOrEmpty(integration.RefreshToken))
                {
                    results.Add(new CalendarEventCreationResult
                    {
                        Provider = integration.Provider.ToString(),
                        Success = false,
                        Error = "Calendar access token expired and cannot be refreshed"
                    });
                    return new CreateCalendarEventResponse { Results = results };
                }

                var refreshResult = await provider.RefreshAccessTokenAsync(integration.RefreshToken, cancellationToken);
                accessToken = refreshResult.AccessToken;
                integration.AccessToken = refreshResult.AccessToken;
                if (!string.IsNullOrEmpty(refreshResult.RefreshToken))
                    integration.RefreshToken = refreshResult.RefreshToken;
                integration.TokenExpiresAt = refreshResult.ExpiresAt;
                await _unitOfWork.SaveChangesAsync(cancellationToken);
            }

            var eventIds = new List<string>();

            if (integration.IncludeBuffers &&
                (request.BufferBeforeMinutes.HasValue && request.BufferBeforeMinutes.Value > 0 ||
                 request.BufferAfterMinutes.HasValue && request.BufferAfterMinutes.Value > 0))
            {
                if (request.BufferBeforeMinutes.HasValue && request.BufferBeforeMinutes.Value > 0)
                {
                    var bufferBeforeDetails = new CalendarEventDetails
                    {
                        Summary = $"[Buffer] Prep for {request.Summary}",
                        StartTime = request.StartTime.AddMinutes(-request.BufferBeforeMinutes.Value),
                        EndTime = request.StartTime,
                        TimeZone = request.TimeZone,
                        AttendeeEmails = new List<string>(),
                        Location = request.Location
                    };
                    var bufferBeforeId = await provider.CreateCalendarEventAsync(accessToken, bufferBeforeDetails, cancellationToken);
                    eventIds.Add(bufferBeforeId);
                }

                var meetingDetails = new CalendarEventDetails
                {
                    Summary = request.Summary,
                    Description = request.Description,
                    StartTime = request.StartTime,
                    EndTime = request.EndTime,
                    TimeZone = request.TimeZone,
                    AttendeeEmails = request.AttendeeEmails,
                    Location = request.Location,
                    MeetingLink = request.MeetingLink
                };
                var meetingEventId = await provider.CreateCalendarEventAsync(accessToken, meetingDetails, cancellationToken);
                eventIds.Add(meetingEventId);

                if (request.BufferAfterMinutes.HasValue && request.BufferAfterMinutes.Value > 0)
                {
                    var bufferAfterDetails = new CalendarEventDetails
                    {
                        Summary = $"[Buffer] Wrap-up for {request.Summary}",
                        StartTime = request.EndTime,
                        EndTime = request.EndTime.AddMinutes(request.BufferAfterMinutes.Value),
                        TimeZone = request.TimeZone,
                        AttendeeEmails = new List<string>(),
                        Location = request.Location
                    };
                    var bufferAfterId = await provider.CreateCalendarEventAsync(accessToken, bufferAfterDetails, cancellationToken);
                    eventIds.Add(bufferAfterId);
                }
            }
            else
            {
                var eventDetails = new CalendarEventDetails
                {
                    Summary = request.Summary,
                    Description = request.Description,
                    StartTime = request.StartTime,
                    EndTime = request.EndTime,
                    TimeZone = request.TimeZone,
                    AttendeeEmails = request.AttendeeEmails,
                    Location = request.Location,
                    MeetingLink = request.MeetingLink
                };
                var eventId = await provider.CreateCalendarEventAsync(accessToken, eventDetails, cancellationToken);
                eventIds.Add(eventId);
            }

            results.Add(new CalendarEventCreationResult
            {
                Provider = integration.Provider.ToString(),
                Success = true,
                EventId = string.Join(",", eventIds)
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating event in {Provider}", integration.Provider);
            results.Add(new CalendarEventCreationResult
            {
                Provider = integration.Provider.ToString(),
                Success = false,
                Error = ex.Message
            });
        }

        return new CreateCalendarEventResponse { Results = results };
    }
}
