using Meetlr.Application.Features.MeetlrEvents.Commands.CreateSingleUseLink;

namespace Meetlr.Api.Endpoints.SingleUseLinks.Create;

public class CreateSingleUseLinkRequest
{
    public Guid MeetlrEventId { get; set; }
    public string? Name { get; set; }
    public DateTime? ExpiresAt { get; set; }

    public static CreateSingleUseLinkCommand ToCommand(CreateSingleUseLinkRequest request)
    {
        return new CreateSingleUseLinkCommand
        {
            MeetlrEventId = request.MeetlrEventId,
            Name = request.Name,
            ExpiresAt = request.ExpiresAt
        };
    }
}
