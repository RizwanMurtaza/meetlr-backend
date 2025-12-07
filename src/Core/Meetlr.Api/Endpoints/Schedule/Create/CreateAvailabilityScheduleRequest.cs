using Meetlr.Application.Features.Schedule.Commands.CreateAvailabilitySchedule;
using Meetlr.Domain.Enums;

namespace Meetlr.Api.Endpoints.Schedule.Create;

public class CreateScheduleRequest
{
    public string Name { get; init; } = string.Empty;
    public string TimeZone { get; init; } = string.Empty;
    public bool IsDefault { get; init; }
    public ScheduleType ScheduleType { get; init; } = ScheduleType.Personal;
    public int MaxBookingsPerSlot { get; init; } = 1;
    public List<WeeklyHourRequest> WeeklyHours { get; init; } = new();

    public static CreateScheduleCommandRequest ToCommand(CreateScheduleRequest request, Guid userId)
    {
        return new CreateScheduleCommandRequest
        {
            UserId = userId,
            Name = request.Name,
            TimeZone = request.TimeZone,
            IsDefault = request.IsDefault,
            ScheduleType = request.ScheduleType,
            MaxBookingsPerSlot = request.MaxBookingsPerSlot,
            WeeklyHours = request.WeeklyHours.Select(w => new WeeklyHourDto
            {
                DayOfWeek = w.DayOfWeek,
                StartTime = w.StartTime,
                EndTime = w.EndTime,
                IsAvailable = w.IsAvailable
            }).ToList()
        };
    }
}
