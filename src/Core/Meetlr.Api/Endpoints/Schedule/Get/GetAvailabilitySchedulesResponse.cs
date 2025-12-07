using Meetlr.Application.Features.Schedule.Queries.GetAvailabilitySchedules;

namespace Meetlr.Api.Endpoints.Schedule.Get;

public class GetAvailabilitySchedulesResponse
{
    public List<AvailabilityScheduleItem> Schedules { get; set; } = new();

    public static GetAvailabilitySchedulesResponse FromQueryResponse(GetAvailabilitySchedulesQueryResponse queryResponse)
    {
        return new GetAvailabilitySchedulesResponse
        {
            Schedules = queryResponse.Schedules.Select(s => new AvailabilityScheduleItem
            {
                Id = s.Id,
                Name = s.Name,
                TimeZone = s.TimeZone,
                IsDefault = s.IsDefault,
                ScheduleType = s.ScheduleType,
                MaxBookingsPerSlot = s.MaxBookingsPerSlot,
                // Advanced Settings
                MaxBookingDaysInFuture = s.MaxBookingDaysInFuture,
                MinBookingNoticeMinutes = s.MinBookingNoticeMinutes,
                SlotIntervalMinutes = s.SlotIntervalMinutes,
                AutoDetectInviteeTimezone = s.AutoDetectInviteeTimezone,
                // Calendar Integrations
                CalendarIntegrations = s.CalendarIntegrations.Select(c => new CalendarIntegrationItem
                {
                    Id = c.Id,
                    Provider = c.Provider,
                    Email = c.Email,
                    IsActive = c.IsActive,
                    IsPrimaryCalendar = c.IsPrimaryCalendar,
                    LastSyncedAt = c.LastSyncedAt
                }).ToList(),
                CreatedAt = s.CreatedAt,
                WeeklyHours = s.WeeklyHours.Select(w => new WeeklyHourItem
                {
                    Id = w.Id,
                    DayOfWeek = w.DayOfWeek,
                    StartTime = w.StartTime,
                    EndTime = w.EndTime,
                    IsAvailable = w.IsAvailable
                }).ToList(),
                DateOverrides = s.DateOverrides.Select(d => new DateOverrideItem
                {
                    Id = d.Id,
                    Date = d.Date,
                    IsAvailable = d.IsAvailable,
                    StartTime = d.StartTime,
                    EndTime = d.EndTime
                }).ToList(),
                MeetlrEvents = s.MeetlrEvents.Select(e => new AvailabilityEventTypeItem
                {
                    Id = e.Id,
                    Name = e.Name,
                    Slug = e.Slug
                }).ToList()
            }).ToList()
        };
    }
}
