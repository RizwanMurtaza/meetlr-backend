using MediatR;
using Meetlr.Application.Common.Interfaces;
using Meetlr.Application.Interfaces;
using Meetlr.Domain.Entities.Scheduling;
using Microsoft.EntityFrameworkCore;

namespace Meetlr.Application.Features.Schedule.Queries.GetAvailabilitySchedules;

public class GetAvailabilitySchedulesQueryHandler : IRequestHandler<GetAvailabilitySchedulesQuery, GetAvailabilitySchedulesQueryResponse>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly ICalendarService? _calendarService;

    public GetAvailabilitySchedulesQueryHandler(
        IUnitOfWork unitOfWork,
        ICalendarService? calendarService = null)
    {
        _unitOfWork = unitOfWork;
        _calendarService = calendarService;
    }

    public async Task<GetAvailabilitySchedulesQueryResponse> Handle(GetAvailabilitySchedulesQuery request, CancellationToken cancellationToken)
    {
        var schedules = await _unitOfWork.Repository<AvailabilitySchedule>().GetQueryable()
            .Include(a => a.WeeklyHours)
            .Include(a => a.DateSpecificHours)
            .Include(a => a.MeetlrEvents)
            .Where(a => a.UserId == request.UserId)
            .OrderByDescending(a => a.IsDefault)
            .ThenByDescending(a => a.CreatedAt)
            .Select(a => new AvailabilityScheduleDto
            {
                Id = a.Id,
                Name = a.Name,
                TimeZone = a.TimeZone,
                IsDefault = a.IsDefault,
                ScheduleType = a.ScheduleType,
                MaxBookingsPerSlot = a.MaxBookingsPerSlot,
                // Advanced Settings
                MaxBookingDaysInFuture = a.MaxBookingDaysInFuture,
                MinBookingNoticeMinutes = a.MinBookingNoticeMinutes,
                SlotIntervalMinutes = a.SlotIntervalMinutes,
                AutoDetectInviteeTimezone = a.AutoDetectInviteeTimezone,
                CreatedAt = a.CreatedAt,
                WeeklyHours = a.WeeklyHours.Select(w => new WeeklyHourDto
                {
                    Id = w.Id,
                    DayOfWeek = w.DayOfWeek,
                    StartTime = w.StartTime,
                    EndTime = w.EndTime,
                    IsAvailable = w.IsAvailable
                }).ToList(),
                DateOverrides = a.DateSpecificHours.Select(d => new DateOverrideDto
                {
                    Id = d.Id,
                    Date = d.Date,
                    IsAvailable = d.IsAvailable,
                    StartTime = d.StartTime,
                    EndTime = d.EndTime
                }).ToList(),
                MeetlrEvents = a.MeetlrEvents.Select(e => new MeetlrEventDto
                {
                    Id = e.Id,
                    Name = e.Name,
                    Slug = e.Slug
                }).ToList()
            })
            .ToListAsync(cancellationToken);

        // Fetch calendar integrations for each schedule using the calendar service
        if (_calendarService != null)
        {
            foreach (var schedule in schedules)
            {
                var calendars = await _calendarService.GetCalendarsForScheduleAsync(schedule.Id, cancellationToken);
                schedule.CalendarIntegrations = calendars.Select(c => new CalendarIntegrationDto
                {
                    Id = c.Id,
                    Provider = c.Provider,
                    Email = c.Email,
                    IsActive = c.IsActive,
                    IsPrimaryCalendar = c.IsPrimaryCalendar,
                    LastSyncedAt = c.LastSyncedAt
                }).ToList();
            }
        }

        return new GetAvailabilitySchedulesQueryResponse
        {
            Schedules = schedules
        };
    }
}
