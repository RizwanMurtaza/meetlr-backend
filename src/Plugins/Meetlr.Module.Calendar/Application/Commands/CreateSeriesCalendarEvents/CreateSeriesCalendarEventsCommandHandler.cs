using MediatR;
using Meetlr.Application.Common.Interfaces;
using Meetlr.Application.Common.Settings;
using Meetlr.Domain.Entities.Events;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace Meetlr.Module.Calendar.Application.Commands.CreateSeriesCalendarEvents;

/// <summary>
/// Command handler for creating calendar events for booking series - 100% pure CQRS with no services
/// </summary>
public class CreateSeriesCalendarEventsCommandHandler
    : IRequestHandler<CreateSeriesCalendarEventsCommand, CreateSeriesCalendarEventsCommandResponse>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IMediator _mediator;
    private readonly RecurringBookingsSettings _settings;
    private readonly ILogger<CreateSeriesCalendarEventsCommandHandler> _logger;

    public CreateSeriesCalendarEventsCommandHandler(
        IUnitOfWork unitOfWork,
        IMediator mediator,
        IOptions<RecurringBookingsSettings> settings,
        ILogger<CreateSeriesCalendarEventsCommandHandler> _logger)
    {
        _unitOfWork = unitOfWork;
        _mediator = mediator;
        _settings = settings.Value;
        this._logger = _logger;
    }

    public async Task<CreateSeriesCalendarEventsCommandResponse> Handle(
        CreateSeriesCalendarEventsCommand request,
        CancellationToken cancellationToken)
    {
        try
        {
            // Get event type
            var eventType = await _unitOfWork.Repository<MeetlrEvent>()
                .GetByIdAsync(request.MeetlrEventId, cancellationToken);

            if (eventType == null)
            {
                _logger.LogWarning("eventType {MeetlrEventId} not found", request.MeetlrEventId);
                return new CreateSeriesCalendarEventsCommandResponse
                {
                    Success = false,
                    ErrorMessage = "Event type not found"
                };
            }

            // Batch calendar event creation with delay to avoid rate limits
            var batchSize = _settings.CalendarBatchSize;
            var delayMs = _settings.CalendarBatchDelayMs;
            var calendarEventIds = new Dictionary<Guid, string>();
            int successCount = 0;
            int failureCount = 0;

            for (int i = 0; i < request.Bookings.Count; i += batchSize)
            {
                var batch = request.Bookings.Skip(i).Take(batchSize);

                foreach (var bookingInfo in batch)
                {
                    try
                    {
                        // Call CreateCalendarEvent command directly via MediatR
                        var calendarEventCommand = new CreateCalendarEvent.CreateCalendarEventCommand
                        {
                            ScheduleId = eventType.AvailabilityScheduleId,
                            Summary = $"{eventType.Name} with {bookingInfo.InviteeName} (Session {i + 1})",
                            Description = $"Recurring booking series\n\nInvitee: {bookingInfo.InviteeName}\nEmail: {bookingInfo.InviteeEmail}\n{(string.IsNullOrEmpty(bookingInfo.Notes) ? "" : $"Notes: {bookingInfo.Notes}\n")}",
                            StartTime = bookingInfo.StartTime,
                            EndTime = bookingInfo.EndTime,
                            TimeZone = eventType.User.TimeZone,
                            AttendeeEmails = new List<string> { bookingInfo.InviteeEmail },
                            Location = bookingInfo.Location,
                            MeetingLink = bookingInfo.MeetingLink
                        };

                        var result = await _mediator.Send(calendarEventCommand, cancellationToken);

                        if (result.Success && result.Results?.Any() == true)
                        {
                            var eventIds = result.Results
                                .Where(r => r.Success && !string.IsNullOrEmpty(r.EventId))
                                .ToDictionary(r => r.Provider, r => r.EventId);

                            if (eventIds.Any())
                            {
                                var calendarEventIdJson = System.Text.Json.JsonSerializer.Serialize(eventIds);
                                calendarEventIds[bookingInfo.BookingId] = calendarEventIdJson;
                                successCount++;
                            }
                            else
                            {
                                failureCount++;
                            }
                        }
                        else
                        {
                            failureCount++;
                        }
                    }
                    catch (Exception ex)
                    {
                        _logger.LogError(ex, "Failed to create calendar event for booking {BookingId}", bookingInfo.BookingId);
                        failureCount++;
                    }
                }

                // Delay between batches
                if (i + batchSize < request.Bookings.Count)
                {
                    await Task.Delay(delayMs, cancellationToken);
                }
            }

            return new CreateSeriesCalendarEventsCommandResponse
            {
                Success = true,
                TotalCreated = successCount,
                TotalFailed = failureCount,
                CalendarEventIds = calendarEventIds
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating series calendar events for eventType {MeetlrEventId}", request.MeetlrEventId);

            return new CreateSeriesCalendarEventsCommandResponse
            {
                Success = false,
                ErrorMessage = ex.Message
            };
        }
    }
}
