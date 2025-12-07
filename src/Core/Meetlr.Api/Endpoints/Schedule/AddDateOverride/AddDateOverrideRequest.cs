using Meetlr.Application.Features.Schedule.Commands.AddDateOverride;

namespace Meetlr.Api.Endpoints.Schedule.AddDateOverride;

public class AddDateOverrideRequest
{
    public DateTime Date { get; init; }
    public bool IsAvailable { get; init; }
    public TimeSpan? StartTime { get; init; }
    public TimeSpan? EndTime { get; init; }

    public static AddDateOverrideCommand ToCommand(AddDateOverrideRequest request, Guid scheduleId, Guid userId)
    {
        return new AddDateOverrideCommand
        {
            AvailabilityScheduleId = scheduleId,
            UserId = userId,
            Date = request.Date,
            IsAvailable = request.IsAvailable,
            StartTime = request.StartTime,
            EndTime = request.EndTime
        };
    }
}
