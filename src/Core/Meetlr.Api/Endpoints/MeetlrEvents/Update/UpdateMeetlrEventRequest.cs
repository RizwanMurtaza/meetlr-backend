using Meetlr.Application.Features.MeetlrEvents.Commands.Update;

namespace Meetlr.Api.Endpoints.MeetlrEvents.Update;

public class UpdateMeetlrEventRequest
{
    // Only these 5 fields are updateable after event creation
    public string? Name { get; init; }
    public string? Color { get; init; }
    public bool? NotifyViaEmail { get; init; }
    public bool? NotifyViaSms { get; init; }
    public bool? NotifyViaWhatsApp { get; init; }

    public static UpdateMeetlrEventCommand ToCommand(UpdateMeetlrEventRequest request, Guid id, Guid userId)
    {
        return new UpdateMeetlrEventCommand
        {
            Id = id,
            UserId = userId,
            Name = request.Name,
            Color = request.Color,
            NotifyViaEmail = request.NotifyViaEmail,
            NotifyViaSms = request.NotifyViaSms,
            NotifyViaWhatsApp = request.NotifyViaWhatsApp
        };
    }
}
