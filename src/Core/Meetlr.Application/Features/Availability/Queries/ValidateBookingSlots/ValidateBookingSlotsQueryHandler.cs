using MediatR;
using Meetlr.Application.Common.Interfaces;
using Meetlr.Application.Features.Availability.Queries.GetAvailableSlots;
using Meetlr.Application.Interfaces;
using Meetlr.Application.Interfaces.Models;
using Meetlr.Domain.Entities.Events;
using Meetlr.Domain.Entities.Scheduling;
using Meetlr.Domain.Enums;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace Meetlr.Application.Features.Availability.Queries.ValidateBookingSlots;

/// <summary>
/// Unified query handler for validating booking slots
/// Handles both single bookings (1 slot) and recurring series (N slots)
/// Checks meeting type (OneOnOne vs Group) and capacity properly
/// </summary>
public class ValidateBookingSlotsQueryHandler
    : IRequestHandler<ValidateBookingSlotsQuery, ValidateBookingSlotsResponse>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IMediator _mediator;
    private readonly ITimeZoneService _timeZoneService;
    private readonly ICalendarService? _calendarService;
    private readonly ILogger<ValidateBookingSlotsQueryHandler> _logger;

    public ValidateBookingSlotsQueryHandler(
        IUnitOfWork unitOfWork,
        IMediator mediator,
        ITimeZoneService timeZoneService,
        ILogger<ValidateBookingSlotsQueryHandler> logger,
        ICalendarService? calendarService = null)
    {
        _unitOfWork = unitOfWork;
        _mediator = mediator;
        _timeZoneService = timeZoneService;
        _calendarService = calendarService;
        _logger = logger;
    }

    /// <summary>
    /// Checks if a slot falls within the availability schedule's working hours
    /// </summary>
    private bool IsSlotWithinAvailability(
        DateTime slotStartUtc,
        DateTime slotEndUtc,
        AvailabilitySchedule schedule,
        string scheduleTimeZone)
    {
        // Convert UTC to schedule's timezone to check against working hours
        var slotStartLocal = _timeZoneService.ConvertFromUtc(slotStartUtc, scheduleTimeZone);
        var slotEndLocal = _timeZoneService.ConvertFromUtc(slotEndUtc, scheduleTimeZone);

        var slotDate = slotStartLocal.Date;
        var slotStartTime = slotStartLocal.TimeOfDay;
        var slotEndTime = slotEndLocal.TimeOfDay;

        _logger.LogInformation(
            "Checking availability: UTC {SlotStartUtc} -> Local {SlotStartLocal} ({DayOfWeek}), TimeZone: {TimeZone}",
            slotStartUtc, slotStartLocal, slotStartLocal.DayOfWeek, scheduleTimeZone);

        // First check date-specific hours (overrides weekly)
        var dateSpecific = schedule.DateSpecificHours?.FirstOrDefault(d => d.Date.Date == slotDate);
        if (dateSpecific != null)
        {
            _logger.LogInformation("Found date-specific hours for {Date}: IsAvailable={IsAvailable}", slotDate, dateSpecific.IsAvailable);
            if (!dateSpecific.IsAvailable)
                return false;
            if (dateSpecific.StartTime.HasValue && dateSpecific.EndTime.HasValue)
            {
                return slotStartTime >= dateSpecific.StartTime.Value &&
                       slotEndTime <= dateSpecific.EndTime.Value;
            }
            return false;
        }

        // Check weekly hours for this day of week
        var dayOfWeek = (DayOfWeekEnum)((int)slotDate.DayOfWeek);
        var weeklyHours = schedule.WeeklyHours?
            .Where(w => w.DayOfWeek == dayOfWeek && w.IsAvailable)
            .ToList();

        _logger.LogInformation(
            "Checking weekly hours for {DayOfWeek}: Found {Count} available windows",
            dayOfWeek, weeklyHours?.Count ?? 0);

        if (weeklyHours == null || !weeklyHours.Any())
        {
            _logger.LogInformation("Day {DayOfWeek} is not available in schedule", dayOfWeek);
            return false; // Day is not available
        }

        // Check if slot fits within any of the available time windows
        var isWithinWindow = weeklyHours.Any(w =>
            slotStartTime >= w.StartTime && slotEndTime <= w.EndTime);

        _logger.LogInformation(
            "Slot {SlotStartTime}-{SlotEndTime} within available windows: {IsWithin}",
            slotStartTime, slotEndTime, isWithinWindow);

        return isWithinWindow;
    }

    public async Task<ValidateBookingSlotsResponse> Handle(
        ValidateBookingSlotsQuery request,
        CancellationToken cancellationToken)
    {
        try
        {
            // Get event type with availability schedule and its weekly/date-specific hours
            var eventType = await _unitOfWork.Repository<MeetlrEvent>()
                .GetQueryable()
                .AsNoTracking()
                .Include(e => e.AvailabilitySchedule)
                    .ThenInclude(s => s!.WeeklyHours)
                .Include(e => e.AvailabilitySchedule)
                    .ThenInclude(s => s!.DateSpecificHours)
                .FirstOrDefaultAsync(e => e.Id == request.MeetlrEventId, cancellationToken)
                .ConfigureAwait(false);

            if (eventType == null)
            {
                _logger.LogWarning("MeetlrEvent {MeetlrEventId} not found", request.MeetlrEventId);
                return new ValidateBookingSlotsResponse
                {
                    HasConflicts = false
                };
            }

            if (!request.RequestedSlots.Any())
            {
                return new ValidateBookingSlotsResponse
                {
                    HasConflicts = false,
                    Conflicts = new List<SlotConflict>()
                };
            }

            // Get schedule timezone for availability checking
            var schedule = eventType.AvailabilitySchedule;
            var scheduleTimeZone = schedule?.TimeZone ?? "UTC";

            // Check if this is a FullDay event - different validation logic
            var isFullDayEvent = eventType.MeetingType == MeetingType.FullDay;

            // PERFORMANCE OPTIMIZATION: Batch database query for entire date range
            var minDate = request.RequestedSlots.Min();
            var maxDate = isFullDayEvent
                ? request.RequestedSlots.Max().AddDays(1) // For FullDay, get entire day
                : request.RequestedSlots.Max().AddMinutes(eventType.DurationMinutes);

            // 1. Get existing bookings from database
            // For FullDay events, group by date; for regular events, group by time slot
            List<dynamic> existingBookingsGrouped;

            if (isFullDayEvent)
            {
                // For FullDay events, group bookings by date
                var bookingsByDate = await _unitOfWork.Repository<Booking>()
                    .GetQueryable()
                    .AsNoTracking()
                    .Where(b =>
                        b.MeetlrEventId == eventType.Id &&
                        b.Status != BookingStatus.Cancelled &&
                        b.StartTime >= minDate.Date &&
                        b.StartTime < maxDate)
                    .GroupBy(b => b.StartTime.Date)
                    .Select(g => new
                    {
                        Date = g.Key,
                        Count = g.Count()
                    })
                    .ToListAsync(cancellationToken)
                    .ConfigureAwait(false);

                existingBookingsGrouped = bookingsByDate.Cast<dynamic>().ToList();
            }
            else
            {
                // For regular events, group by time slot
                var bookingsBySlot = await _unitOfWork.Repository<Booking>()
                    .GetQueryable()
                    .AsNoTracking()
                    .Where(b =>
                        b.MeetlrEventId == eventType.Id &&
                        b.Status != BookingStatus.Cancelled &&
                        b.StartTime < maxDate &&
                        b.EndTime > minDate)
                    .GroupBy(b => new { b.StartTime, b.EndTime })
                    .Select(g => new
                    {
                        g.Key.StartTime,
                        g.Key.EndTime,
                        Count = g.Count()
                    })
                    .ToListAsync(cancellationToken)
                    .ConfigureAwait(false);

                existingBookingsGrouped = bookingsBySlot.Cast<dynamic>().ToList();
            }

            // 2. Get calendar busy times from external calendars (skip for FullDay events)
            var calendarBusyTimes = new List<CalendarBusySlot>();
            if (!isFullDayEvent && _calendarService != null)
            {
                try
                {
                    calendarBusyTimes = await _calendarService.GetBusyTimesAsync(
                        eventType.UserId,
                        minDate,
                        maxDate,
                        cancellationToken);
                }
                catch (Exception ex)
                {
                    _logger.LogWarning(ex, "Failed to get calendar busy times");
                }
            }

            // 3. Check each requested slot
            var conflicts = new List<SlotConflict>();

            for (int i = 0; i < request.RequestedSlots.Count; i++)
            {
                var slotStart = request.RequestedSlots[i];
                var slotEnd = slotStart.AddMinutes(eventType.DurationMinutes);
                string? conflictReason = null;

                // First check if slot falls within availability schedule
                if (schedule != null && !isFullDayEvent)
                {
                    if (!IsSlotWithinAvailability(slotStart, slotEnd, schedule, scheduleTimeZone))
                    {
                        conflictReason = "This time is outside available hours";
                    }
                }

                if (conflictReason == null && isFullDayEvent)
                {
                    // FullDay: Check capacity by date only
                    var requestedDate = slotStart.Date;
                    var bookingsOnDate = existingBookingsGrouped
                        .Where(b => ((DateTime)b.Date).Date == requestedDate)
                        .Sum(b => (int)b.Count);

                    if (eventType.MaxAttendeesPerSlot.HasValue &&
                        bookingsOnDate >= eventType.MaxAttendeesPerSlot.Value)
                    {
                        conflictReason = $"This date is fully booked ({bookingsOnDate}/{eventType.MaxAttendeesPerSlot.Value} attendees)";
                    }
                }
                else if (conflictReason == null)
                {
                    // Non-FullDay: Check booking conflicts and capacity
                    // Check based on meeting type
                    if (eventType.MeetingType == MeetingType.OneOnOne)
                    {
                        // OneOnOne: Check if ANY booking exists in this time slot
                        var hasBookingConflict = existingBookingsGrouped.Any(b =>
                            (DateTime)b.StartTime < slotEnd && (DateTime)b.EndTime > slotStart);

                        if (hasBookingConflict)
                        {
                            conflictReason = "This time slot is already booked";
                        }
                    }
                    else if (eventType.MeetingType == MeetingType.Group ||
                             eventType.MeetingType == MeetingType.OneOff)
                    {
                        // Group/OneOff: Check if capacity is reached
                        var bookingsInSlot = existingBookingsGrouped
                            .Where(b => (DateTime)b.StartTime < slotEnd && (DateTime)b.EndTime > slotStart)
                            .Sum(b => (int)b.Count);

                        if (eventType.MaxAttendeesPerSlot.HasValue &&
                            bookingsInSlot >= eventType.MaxAttendeesPerSlot.Value)
                        {
                            conflictReason = $"This event is fully booked ({bookingsInSlot}/{eventType.MaxAttendeesPerSlot.Value} attendees)";
                        }
                    }

                    // Also check external calendar conflicts (applies to non-FullDay types)
                    if (conflictReason == null)
                    {
                        var hasCalendarConflict = calendarBusyTimes.Any(bt =>
                            bt.StartTime < slotEnd && bt.EndTime > slotStart);

                        if (hasCalendarConflict)
                        {
                            conflictReason = "This time conflicts with another calendar event";
                        }
                    }
                }

                // If conflict found, add to conflicts list
                if (conflictReason != null)
                {
                    conflicts.Add(new SlotConflict
                    {
                        SlotIndex = i,
                        RequestedTime = slotStart,
                        ConflictReason = conflictReason,
                        SuggestedAlternatives = new List<AlternativeSlotDto>() // Will be populated below
                    });
                }
            }

            // Batch load alternative slots for all conflicts at once
            if (conflicts.Any())
            {
                await PopulateAlternativeSlotsAsync(eventType, conflicts, request.TimeZone, cancellationToken);
            }

            return new ValidateBookingSlotsResponse
            {
                HasConflicts = conflicts.Any(),
                Conflicts = conflicts,
                Message = conflicts.Any()
                    ? $"{conflicts.Count} of {request.RequestedSlots.Count} time slots are unavailable"
                    : "All time slots are available"
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error validating booking slots for MeetlrEvent {MeetlrEventId}", request.MeetlrEventId);

            return new ValidateBookingSlotsResponse
            {
                HasConflicts = false
            };
        }
    }

    /// <summary>
    /// Batch populate alternative slots for all conflicts
    /// </summary>
    private async Task PopulateAlternativeSlotsAsync(
        MeetlrEvent eventType,
        List<SlotConflict> conflicts,
        string timeZone,
        CancellationToken cancellationToken)
    {
        // Get unique dates that have conflicts
        var conflictDates = conflicts.Select(c => c.RequestedTime.Date).Distinct().ToList();

        // Batch load available slots for all conflict dates
        var allSlotsTasks = conflictDates.Select(async date =>
        {
            try
            {
                var query = new GetAvailableSlotsQuery
                {
                    MeetlrEventId = eventType.Id,
                    StartDate = date,
                    EndDate = date.AddDays(1),
                    TimeZone = timeZone
                };

                var result = await _mediator.Send(query, cancellationToken).ConfigureAwait(false);
                return new { Date = date, Slots = result.Slots };
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Failed to get alternative slots for date {Date}", date);
                return new { Date = date, Slots = new List<AvailableSlotDto>() };
            }
        });

        var allSlotsResults = await Task.WhenAll(allSlotsTasks).ConfigureAwait(false);
        var slotsLookup = allSlotsResults.ToDictionary(r => r.Date, r => r.Slots);

        // Populate suggested slots for each conflict
        foreach (var conflict in conflicts)
        {
            if (slotsLookup.TryGetValue(conflict.RequestedTime.Date, out var availableSlots))
            {
                conflict.SuggestedAlternatives = availableSlots
                    .OrderBy(s => Math.Abs((s.StartTime - conflict.RequestedTime).TotalMinutes))
                    .Take(3)
                    .Select(s => new AlternativeSlotDto
                    {
                        StartTime = s.StartTime,
                        EndTime = s.EndTime,
                        DisplayTime = _timeZoneService.ConvertFromUtc(s.StartTime, timeZone).ToString("h:mm tt")
                    })
                    .ToList();
            }
        }
    }
}
