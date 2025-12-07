using MediatR;
using Meetlr.Application.Interfaces;
using Microsoft.Extensions.Logging;

namespace Meetlr.Application.Features.Availability.Queries.CheckCalendarBusyTimes;

/// <summary>
/// Query handler for checking calendar busy times using calendar service
/// </summary>
public class CheckCalendarBusyTimesQueryHandler
    : IRequestHandler<CheckCalendarBusyTimesQuery, CheckCalendarBusyTimesQueryResponse>
{
    private readonly ICalendarService _calendarService;
    private readonly ILogger<CheckCalendarBusyTimesQueryHandler> _logger;

    public CheckCalendarBusyTimesQueryHandler(
        ICalendarService calendarService,
        ILogger<CheckCalendarBusyTimesQueryHandler> logger)
    {
        _calendarService = calendarService;
        _logger = logger;
    }

    public async Task<CheckCalendarBusyTimesQueryResponse> Handle(
        CheckCalendarBusyTimesQuery request,
        CancellationToken cancellationToken)
    {
        try
        {
            _logger.LogInformation(
                "Checking calendar conflicts for user {UserId} at {StartTime}",
                request.UserId,
                request.StartTimeUtc);

            // Get busy times from all connected calendars
            var busySlots = await _calendarService.GetBusyTimesAsync(
                request.UserId,
                request.StartTimeUtc,
                request.EndTimeUtc,
                cancellationToken);

            // Filter for actual conflicts with the requested time range
            var conflicts = busySlots
                .Where(slot =>
                    slot.StartTime < request.EndTimeUtc &&
                    slot.EndTime > request.StartTimeUtc)
                .ToList();

            if (conflicts.Count > 0)
            {
                _logger.LogWarning(
                    "Calendar conflict detected: {BusyTimeCount} busy time(s) found",
                    conflicts.Count);

                return new CheckCalendarBusyTimesQueryResponse
                {
                    HasConflicts = true,
                    BusyTimeCount = conflicts.Count,
                    ConflictReason = $"Host has {conflicts.Count} conflicting calendar event(s)"
                };
            }

            _logger.LogInformation("No calendar conflicts found");

            return new CheckCalendarBusyTimesQueryResponse
            {
                HasConflicts = false,
                BusyTimeCount = 0,
                ConflictReason = null
            };
        }
        catch (Exception ex)
        {
            // Log calendar check failure but don't block booking
            _logger.LogWarning(ex, "Failed to check calendar conflicts");

            return new CheckCalendarBusyTimesQueryResponse
            {
                HasConflicts = false,
                BusyTimeCount = 0,
                ConflictReason = "Unable to check calendar - proceeding without conflict check"
            };
        }
    }
}
