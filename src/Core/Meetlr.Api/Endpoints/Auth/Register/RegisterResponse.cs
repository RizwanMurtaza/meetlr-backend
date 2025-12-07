using Meetlr.Application.Common.Settings;
using Meetlr.Application.Features.Authentication.Commands.Register;

namespace Meetlr.Api.Endpoints.Auth.Register;

public record RegisterResponse
{
    public Guid UserId { get; init; }
    public string Email { get; init; } = string.Empty;
    public string FirstName { get; init; } = string.Empty;
    public string LastName { get; init; } = string.Empty;
    public string MeetlrUsername { get; init; } = string.Empty;
    public string BookingUrl { get; init; } = string.Empty;

    public static RegisterResponse FromCommandResponse(RegisterCommandResponse response, ApplicationUrlsSettings urlsSettings)
    {
        return new RegisterResponse
        {
            UserId = response.UserId,
            Email = response.Email,
            FirstName = response.FirstName,
            LastName = response.LastName,
            MeetlrUsername = response.MeetlrUsername,
            BookingUrl = urlsSettings.BuildBookingUrl(response.MeetlrUsername)
        };
    }
}
