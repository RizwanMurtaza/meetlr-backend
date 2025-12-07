using MediatR;
using Meetlr.Application.Common.Interfaces;
using Meetlr.Application.Interfaces;
using Meetlr.Application.Models;
using Meetlr.Domain.Entities.Events;
using Meetlr.Domain.Entities.Scheduling;
using Meetlr.Domain.Enums;
using Meetlr.Domain.Exceptions.DomainErrors.ExceptionByArea;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace Meetlr.Application.Features.Availability.Queries.GetAvailableSlots;

/// <summary>
/// Context for slot generation containing all necessary data.
/// Uses sorted lists for O(log n) conflict detection via binary search.
/// </summary>
internal sealed class SlotGenerationContext
{
    public MeetlrEvent EventType { get; }
    public AvailabilitySchedule Schedule { get; }
    public string ScheduleTimeZone { get; }
    public string DisplayTimeZone { get; }

    // Effective settings (schedule overrides event defaults)
    public int MinBookingNoticeMinutes { get; }
    public int SlotIntervalMinutes { get; }
    public int MaxBookingDaysInFuture { get; }

    // Sorted by StartTime for binary search optimization
    private readonly List<TimeSlotDto> _existingBookings;
    private readonly List<BusyTimeSlotDto> _calendarBusyTimes;
    private readonly List<SlotReservationInfo> _slotReservations;

    public SlotGenerationContext(
        MeetlrEvent eventType,
        AvailabilitySchedule schedule,
        List<TimeSlotDto> existingBookings,
        List<BusyTimeSlotDto> calendarBusyTimes,
        List<SlotReservationInfo> slotReservations,
        string scheduleTimeZone,
        string displayTimeZone)
    {
        EventType = eventType;
        Schedule = schedule;
        ScheduleTimeZone = scheduleTimeZone;
        DisplayTimeZone = displayTimeZone;

        // Use event's slot interval if set, otherwise fall back to schedule's setting
        MinBookingNoticeMinutes = schedule.MinBookingNoticeMinutes;
        SlotIntervalMinutes = eventType.SlotIntervalMinutes > 0 ? eventType.SlotIntervalMinutes : schedule.SlotIntervalMinutes;
        MaxBookingDaysInFuture = schedule.MaxBookingDaysInFuture;

        // Sort once during construction for efficient conflict checking
        _existingBookings = existingBookings.OrderBy(b => b.StartTime).ToList();
        _calendarBusyTimes = calendarBusyTimes.OrderBy(b => b.StartTime).ToList();
        _slotReservations = slotReservations.OrderBy(r => r.StartTime).ToList();
    }

    /// <summary>
    /// Check for any conflicts using early-exit optimization.
    /// Since lists are sorted by StartTime, we can skip items that end before our window.
    /// </summary>
    public bool HasConflict(DateTime slotStart, DateTime slotEnd)
    {
        var bufferStart = slotStart.AddMinutes(-EventType.BufferTimeBeforeMinutes);
        var bufferEnd = slotEnd.AddMinutes(EventType.BufferTimeAfterMinutes);

        // Check bookings with early exit
        foreach (var b in _existingBookings)
        {
            if (b.EndTime <= bufferStart) continue; // Too early, skip
            if (b.StartTime >= bufferEnd) break;    // Past our window, stop
            return true; // Overlap found
        }

        // Check calendar busy times with early exit
        foreach (var bt in _calendarBusyTimes)
        {
            if (bt.EndTime <= bufferStart) continue;
            if (bt.StartTime >= bufferEnd) break;
            return true;
        }

        // Check reservations for non-group events
        if (!EventType.MaxAttendeesPerSlot.HasValue || EventType.MaxAttendeesPerSlot.Value == 1)
        {
            foreach (var r in _slotReservations)
            {
                if (r.EndTime <= bufferStart) continue;
                if (r.StartTime >= bufferEnd) break;
                return true;
            }
        }

        return false;
    }
}

/// <summary>
/// Availability lookup dictionaries for O(1) access
/// </summary>
internal record AvailabilityLookup(
    Dictionary<DateTime, DateSpecificHours> DateSpecific,
    Dictionary<DayOfWeekEnum, List<WeeklyHours>> Weekly);

/// <summary>
/// Handler for getting available time slots
/// </summary>
public class GetAvailableSlotsQueryHandler : IRequestHandler<GetAvailableSlotsQuery, GetAvailableSlotsQueryResponse>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly ITimeZoneService _timeZoneService;
    private readonly ICalendarService _calendarService;
    private readonly ISlotInvitationService? _slotInvitationService;
    private readonly ILogger<GetAvailableSlotsQueryHandler> _logger;

    public GetAvailableSlotsQueryHandler(
        IUnitOfWork unitOfWork,
        ITimeZoneService timeZoneService,
        ICalendarService calendarService,
        ILogger<GetAvailableSlotsQueryHandler> logger,
        ISlotInvitationService? slotInvitationService = null)
    {
        _unitOfWork = unitOfWork;
        _timeZoneService = timeZoneService;
        _calendarService = calendarService;
        _slotInvitationService = slotInvitationService;
        _logger = logger;
    }

    public async Task<GetAvailableSlotsQueryResponse> Handle(GetAvailableSlotsQuery request, CancellationToken cancellationToken)
    {
        var eventType = await LoadEventTypeAsync(request.MeetlrEventId, cancellationToken);
        var availabilitySchedule = eventType.AvailabilitySchedule!;

        var scheduleTimeZone = availabilitySchedule.TimeZone;

        // If AutoDetectInviteeTimezone is false, lock display to schedule's timezone
        // Otherwise, use the invitee's requested timezone (or fall back to user's timezone)
        var displayTimeZone = availabilitySchedule.AutoDetectInviteeTimezone
            ? (request.TimeZone ?? eventType.User.TimeZone)
            : scheduleTimeZone;

        // FullDay events have different slot generation logic
        if (eventType.MeetingType == MeetingType.FullDay)
        {
            return await GenerateFullDaySlotsAsync(eventType, request, displayTimeZone, availabilitySchedule, cancellationToken);
        }

        var context = await BuildSlotGenerationContextAsync(eventType, availabilitySchedule, request, scheduleTimeZone, displayTimeZone, cancellationToken);
        var lookup = BuildAvailabilityLookup(availabilitySchedule);
        var slots = GenerateTimeSlotsForDateRange(request.StartDate, request.EndDate, context, lookup);

        return new GetAvailableSlotsQueryResponse
        {
            Slots = slots.OrderBy(s => s.StartTime).ToList(),
            TimeZone = displayTimeZone,
            MeetingType = eventType.MeetingType,
            IsFullDayEvent = false,
            MaxAttendeesPerSlot = eventType.MaxAttendeesPerSlot,
            DurationMinutes = eventType.DurationMinutes
        };
    }

    private async Task<MeetlrEvent> LoadEventTypeAsync(Guid meetlrEventId, CancellationToken cancellationToken)
    {
        var eventType = await _unitOfWork.Repository<MeetlrEvent>().GetQueryable()
            .AsNoTracking()
            .Include(e => e.AvailabilitySchedule!)
                .ThenInclude(a => a.WeeklyHours)
            .Include(e => e.AvailabilitySchedule!)
                .ThenInclude(a => a.DateSpecificHours)
            .Include(e => e.User)
            .FirstOrDefaultAsync(e => e.Id == meetlrEventId, cancellationToken)
            .ConfigureAwait(false);

        if (eventType == null)
            throw MeetlrEventErrors.MeetlrEventNotFound(meetlrEventId);

        if (eventType.AvailabilitySchedule == null)
            throw AvailabilityErrors.AvailabilityScheduleNotFound(eventType.AvailabilityScheduleId);

        return eventType;
    }

    private async Task<SlotGenerationContext> BuildSlotGenerationContextAsync(
        MeetlrEvent eventType,
        AvailabilitySchedule availabilitySchedule,
        GetAvailableSlotsQuery request,
        string scheduleTimeZone,
        string displayTimeZone,
        CancellationToken cancellationToken)
    {
        var endDatePlusOne = request.EndDate.AddDays(1);

        // Get bookings for all events sharing this schedule (prevents double-booking)
        var eventIdsWithSameSchedule = await _unitOfWork.Repository<MeetlrEvent>().GetQueryable()
            .AsNoTracking()
            .Where(e => e.UserId == eventType.UserId && e.AvailabilityScheduleId == availabilitySchedule.Id)
            .Select(e => e.Id)
            .ToListAsync(cancellationToken)
            .ConfigureAwait(false);

        // Run independent queries in parallel
        var bookingsTask = _unitOfWork.Repository<Booking>().GetQueryable()
            .AsNoTracking()
            .Where(b => eventIdsWithSameSchedule.Contains(b.MeetlrEventId))
            .Where(b => b.Status == BookingStatus.Confirmed || b.Status == BookingStatus.Pending)
            .Where(b => b.StartTime >= request.StartDate && b.StartTime < endDatePlusOne)
            .Select(b => new TimeSlotDto(b.StartTime, b.EndTime))
            .ToListAsync(cancellationToken);

        var calendarTask = GetCalendarBusyTimesAsync(eventType.UserId, request.StartDate, endDatePlusOne, cancellationToken);
        var reservationsTask = GetSlotReservationsAsync(eventType.Id, request.StartDate, endDatePlusOne, cancellationToken);

        await Task.WhenAll(bookingsTask, calendarTask, reservationsTask).ConfigureAwait(false);

        return new SlotGenerationContext(
            eventType,
            availabilitySchedule,
            await bookingsTask,
            await calendarTask,
            await reservationsTask,
            scheduleTimeZone,
            displayTimeZone);
    }

    private async Task<List<BusyTimeSlotDto>> GetCalendarBusyTimesAsync(Guid userId, DateTime startDate, DateTime endDate, CancellationToken cancellationToken)
    {
        try
        {
            var busySlots = await _calendarService.GetBusyTimesAsync(userId, startDate, endDate, cancellationToken);
            return busySlots.Select(s => new BusyTimeSlotDto(s.StartTime, s.EndTime)).ToList();
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Failed to get calendar busy times");
            return [];
        }
    }

    private async Task<List<SlotReservationInfo>> GetSlotReservationsAsync(Guid eventId, DateTime startDate, DateTime endDate, CancellationToken cancellationToken)
    {
        if (_slotInvitationService == null) return [];

        try
        {
            return await _slotInvitationService.GetActiveReservationsAsync(eventId, startDate, endDate, cancellationToken);
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Failed to get slot reservations");
            return [];
        }
    }

    private static AvailabilityLookup BuildAvailabilityLookup(AvailabilitySchedule schedule) => new(
        schedule.DateSpecificHours.ToDictionary(d => d.Date.Date, d => d),
        schedule.WeeklyHours.Where(w => w.IsAvailable).GroupBy(w => w.DayOfWeek).ToDictionary(g => g.Key, g => g.ToList())
    );

    private List<AvailableSlotDto> GenerateTimeSlotsForDateRange(DateTime startDate, DateTime endDate, SlotGenerationContext context, AvailabilityLookup lookup)
    {
        var slots = new List<AvailableSlotDto>();
        var currentDate = startDate.Date;

        while (currentDate <= endDate.Date)
        {
            GenerateSlotsForDate(currentDate, context, lookup, slots);
            currentDate = currentDate.AddDays(1);
        }

        return slots;
    }

    private void GenerateSlotsForDate(DateTime date, SlotGenerationContext context, AvailabilityLookup lookup, List<AvailableSlotDto> slots)
    {
        if (lookup.DateSpecific.TryGetValue(date, out var dateSpecific))
        {
            if (dateSpecific.IsAvailable && dateSpecific.StartTime.HasValue && dateSpecific.EndTime.HasValue)
            {
                GenerateSlots(date, dateSpecific.StartTime.Value, dateSpecific.EndTime.Value, context, slots);
            }
            return;
        }

        var dayOfWeek = (DayOfWeekEnum)((int)date.DayOfWeek);
        if (lookup.Weekly.TryGetValue(dayOfWeek, out var weeklyHours))
        {
            foreach (var hours in weeklyHours)
            {
                GenerateSlots(date, hours.StartTime, hours.EndTime, context, slots);
            }
        }
    }

    private async Task<GetAvailableSlotsQueryResponse> GenerateFullDaySlotsAsync(
        MeetlrEvent eventType,
        GetAvailableSlotsQuery request,
        string timeZone,
        AvailabilitySchedule availabilitySchedule,
        CancellationToken cancellationToken)
    {
        var lookup = BuildAvailabilityLookup(availabilitySchedule);
        var endDatePlusOne = request.EndDate.AddDays(1);
        var today = DateTime.UtcNow.Date;

        // Apply schedule's advanced settings
        var minNoticeDate = DateTime.UtcNow.AddMinutes(availabilitySchedule.MinBookingNoticeMinutes).Date;
        var maxFutureDate = DateTime.UtcNow.AddDays(availabilitySchedule.MaxBookingDaysInFuture).Date;

        var bookingCountsByDate = await GetBookingCountsByDateAsync(eventType.Id, request.StartDate, endDatePlusOne, cancellationToken);
        var reservationCountsByDate = await GetReservationCountsByDateAsync(eventType.Id, request.StartDate, endDatePlusOne, cancellationToken);

        var slots = new List<AvailableSlotDto>();
        var currentDate = request.StartDate.Date;

        while (currentDate <= request.EndDate.Date)
        {
            // Apply minimum notice and max booking window from schedule
            if (currentDate >= minNoticeDate && currentDate <= maxFutureDate && IsDateAvailable(currentDate, lookup))
            {
                var currentBookings = bookingCountsByDate.GetValueOrDefault(currentDate, 0);
                var reservedSpots = reservationCountsByDate.GetValueOrDefault(currentDate, 0);
                var hasCapacity = !eventType.MaxAttendeesPerSlot.HasValue ||
                                  (currentBookings + reservedSpots) < eventType.MaxAttendeesPerSlot.Value;

                slots.Add(new AvailableSlotDto
                {
                    StartTime = currentDate,
                    EndTime = currentDate.AddDays(1).AddTicks(-1),
                    IsAvailable = hasCapacity,
                    IsFullDay = true,
                    CurrentBookings = currentBookings,
                    MaxCapacity = eventType.MaxAttendeesPerSlot
                });
            }
            currentDate = currentDate.AddDays(1);
        }

        return new GetAvailableSlotsQueryResponse
        {
            Slots = slots.OrderBy(s => s.StartTime).ToList(),
            TimeZone = timeZone,
            MeetingType = eventType.MeetingType,
            IsFullDayEvent = true,
            MaxAttendeesPerSlot = eventType.MaxAttendeesPerSlot,
            DurationMinutes = eventType.DurationMinutes
        };
    }

    private async Task<Dictionary<DateTime, int>> GetBookingCountsByDateAsync(Guid eventId, DateTime startDate, DateTime endDate, CancellationToken cancellationToken)
    {
        return await _unitOfWork.Repository<Booking>().GetQueryable()
            .AsNoTracking()
            .Where(b => b.MeetlrEventId == eventId)
            .Where(b => b.Status == BookingStatus.Confirmed || b.Status == BookingStatus.Pending)
            .Where(b => b.StartTime >= startDate && b.StartTime < endDate)
            .GroupBy(b => b.StartTime.Date)
            .Select(g => new { Date = g.Key, Count = g.Count() })
            .ToDictionaryAsync(x => x.Date, x => x.Count, cancellationToken)
            .ConfigureAwait(false);
    }

    private async Task<Dictionary<DateTime, int>> GetReservationCountsByDateAsync(Guid eventId, DateTime startDate, DateTime endDate, CancellationToken cancellationToken)
    {
        if (_slotInvitationService == null) return [];

        try
        {
            var reservations = await _slotInvitationService.GetActiveReservationsAsync(eventId, startDate, endDate, cancellationToken);
            return reservations.GroupBy(r => r.StartTime.Date).ToDictionary(g => g.Key, g => g.Sum(r => r.SpotsReserved));
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Failed to get slot reservations for full-day event");
            return [];
        }
    }

    private static bool IsDateAvailable(DateTime date, AvailabilityLookup lookup)
    {
        if (lookup.DateSpecific.TryGetValue(date, out var dateSpecific))
            return dateSpecific.IsAvailable;

        var dayOfWeek = (DayOfWeekEnum)((int)date.DayOfWeek);
        return lookup.Weekly.ContainsKey(dayOfWeek);
    }

    private void GenerateSlots(DateTime date, TimeSpan startTime, TimeSpan endTime, SlotGenerationContext ctx, List<AvailableSlotDto> slots)
    {
        var startUtc = _timeZoneService.ConvertToUtc(date.Add(startTime), ctx.ScheduleTimeZone);
        var endUtc = _timeZoneService.ConvertToUtc(date.Add(endTime), ctx.ScheduleTimeZone);

        // Use schedule's minimum notice setting
        var minimumTime = DateTime.UtcNow.AddMinutes(ctx.MinBookingNoticeMinutes);
        if (endUtc < minimumTime) return;

        // Enforce max booking days in future (booking window)
        var maxFutureTime = DateTime.UtcNow.AddDays(ctx.MaxBookingDaysInFuture);
        if (startUtc > maxFutureTime) return;

        var slotDuration = ctx.EventType.DurationMinutes;
        var slotInterval = ctx.SlotIntervalMinutes > 0 ? ctx.SlotIntervalMinutes : slotDuration;
        var currentSlotStart = startUtc;

        while (currentSlotStart.AddMinutes(slotDuration) <= endUtc)
        {
            // Skip slots beyond the booking window
            if (currentSlotStart > maxFutureTime) break;

            if (currentSlotStart >= minimumTime)
            {
                var slotEnd = currentSlotStart.AddMinutes(slotDuration);

                if (!ctx.HasConflict(currentSlotStart, slotEnd))
                {
                    slots.Add(new AvailableSlotDto
                    {
                        StartTime = _timeZoneService.ConvertFromUtc(currentSlotStart, ctx.DisplayTimeZone),
                        EndTime = _timeZoneService.ConvertFromUtc(slotEnd, ctx.DisplayTimeZone),
                        IsAvailable = true
                    });
                }
            }
            currentSlotStart = currentSlotStart.AddMinutes(slotInterval);
        }
    }
}

/// <summary>
/// DTO for busy time slots from calendar
/// </summary>
public record BusyTimeSlotDto(DateTime StartTime, DateTime EndTime);
