using Meetlr.Application.Features.MeetlrEvents.Commands.CreateSingleUseLink;

namespace Meetlr.Api.Endpoints.SingleUseLinks.Create;

public class CreateSingleUseLinkResponse
{
    public Guid Id { get; set; }
    public string Token { get; set; } = string.Empty;
    public string BookingUrl { get; set; } = string.Empty;
    public string? Name { get; set; }
    public DateTime? ExpiresAt { get; set; }
    public DateTime CreatedAt { get; set; }

    public static CreateSingleUseLinkResponse FromCommandResponse(CreateSingleUseLinkCommandResponse commandResponse)
    {
        return new CreateSingleUseLinkResponse
        {
            Id = commandResponse.Id,
            Token = commandResponse.Token,
            BookingUrl = commandResponse.BookingUrl,
            Name = commandResponse.Name,
            ExpiresAt = commandResponse.ExpiresAt,
            CreatedAt = commandResponse.CreatedAt
        };
    }
}
