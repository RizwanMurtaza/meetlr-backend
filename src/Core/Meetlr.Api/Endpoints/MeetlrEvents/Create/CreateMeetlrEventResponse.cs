using Meetlr.Application.Features.MeetlrEvents.Commands.Create;

namespace Meetlr.Api.Endpoints.MeetlrEvents.Create;

public class CreateMeetlrEventResponse
{
    public Guid Id { get; init; }
    public string Name { get; init; } = string.Empty;
    public string? Description { get; init; }
    public int DurationMinutes { get; init; }
    public string? SlugUrl { get; init; }
    public string BookingUrl { get; init; } = string.Empty;
    public bool RequiresPayment { get; init; }
    public decimal? Fee { get; init; }
    public string? Currency { get; init; }
    public DateTime CreatedAt { get; init; }

    public static CreateMeetlrEventResponse FromCommandResponse(CreateMeetlrEventCommandResponse commandResponse)
    {
        return new CreateMeetlrEventResponse
        {
            Id = commandResponse.Id,
            Name = commandResponse.Name,
            Description = commandResponse.Description,
            DurationMinutes = commandResponse.DurationMinutes,
            SlugUrl = commandResponse.SlugUrl,
            BookingUrl = commandResponse.BookingUrl,
            RequiresPayment = commandResponse.RequiresPayment,
            Fee = commandResponse.Fee,
            Currency = commandResponse.Currency,
            CreatedAt = commandResponse.CreatedAt
        };
    }
}
