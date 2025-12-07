using MediatR;

namespace Meetlr.Application.Features.Authentication.Commands.Register;

/// <summary>
/// Command for user registration
/// </summary>
public record RegisterCommand : IRequest<RegisterCommandResponse>
{
    public string FirstName { get; init; } = string.Empty;
    public string LastName { get; init; } = string.Empty;
    public string Email { get; init; } = string.Empty;
    public string Password { get; init; } = string.Empty;
    public string? MeetlrUsername { get; init; }
    public string TimeZone { get; init; } = "UTC";
}
