using Meetlr.Application.Features.Authentication.Commands.Register;

namespace Meetlr.Api.Endpoints.Auth.Register;

public record RegisterRequest
{
    public string FirstName { get; init; } = string.Empty;
    public string LastName { get; init; } = string.Empty;
    public string Email { get; init; } = string.Empty;
    public string Password { get; init; } = string.Empty;
    public string? MeetlrUsername { get; init; }
    public string TimeZone { get; init; } = "UTC";

    public static RegisterCommand ToCommand(RegisterRequest request)
    {
        return new RegisterCommand
        {
            FirstName = request.FirstName,
            LastName = request.LastName,
            Email = request.Email,
            Password = request.Password,
            MeetlrUsername = request.MeetlrUsername,
            TimeZone = request.TimeZone
        };
    }
}
