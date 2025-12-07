using MediatR;

namespace Meetlr.Application.Features.MeetlrEvents.Commands.CreateSingleUseLink;

public class CreateSingleUseLinkCommand : IRequest<CreateSingleUseLinkCommandResponse>
{
    public Guid MeetlrEventId { get; set; }
    public string? Name { get; set; } // Optional label (e.g., "For John Smith")
    public DateTime? ExpiresAt { get; set; } // Optional expiration
}
