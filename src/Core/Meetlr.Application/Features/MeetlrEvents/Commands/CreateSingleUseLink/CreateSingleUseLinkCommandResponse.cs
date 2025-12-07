namespace Meetlr.Application.Features.MeetlrEvents.Commands.CreateSingleUseLink;

public class CreateSingleUseLinkCommandResponse
{
    public Guid Id { get; set; }
    public string Token { get; set; } = string.Empty;
    public string BookingUrl { get; set; } = string.Empty;
    public string? Name { get; set; }
    public DateTime? ExpiresAt { get; set; }
    public DateTime CreatedAt { get; set; }
}
