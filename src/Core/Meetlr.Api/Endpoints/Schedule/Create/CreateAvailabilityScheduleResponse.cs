using Meetlr.Application.Features.Schedule.Commands.CreateAvailabilitySchedule;

namespace Meetlr.Api.Endpoints.Schedule.Create;

public class CreateAvailabilityScheduleResponse
{
    public Guid Id { get; init; }
    public string Name { get; init; } = string.Empty;
    public string TimeZone { get; init; } = string.Empty;
    public bool IsDefault { get; init; }
    public bool Success { get; init; }

    public static CreateAvailabilityScheduleResponse FromCommandResponse(CreateScheduleCommandResponse commandResponse)
    {
        return new CreateAvailabilityScheduleResponse
        {
            Id = commandResponse.Id,
            Name = commandResponse.Name,
            TimeZone = commandResponse.TimeZone,
            IsDefault = commandResponse.IsDefault,
            Success = commandResponse.Success
        };
    }
}
