using Meetlr.Application.Features.Schedule.Commands.AddDateOverride;

namespace Meetlr.Api.Endpoints.Schedule.AddDateOverride;

public class AddDateOverrideResponse
{
    public Guid Id { get; init; }
    public DateTime Date { get; init; }
    public bool IsAvailable { get; init; }
    public bool Success { get; init; }

    public static AddDateOverrideResponse FromCommandResponse(AddDateOverrideCommandResponse commandResponse)
    {
        return new AddDateOverrideResponse
        {
            Id = commandResponse.Id,
            Date = commandResponse.Date,
            IsAvailable = commandResponse.IsAvailable,
            Success = commandResponse.Success
        };
    }
}
