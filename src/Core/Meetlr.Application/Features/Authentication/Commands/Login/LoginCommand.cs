using MediatR;

namespace Meetlr.Application.Features.Authentication.Commands.Login;

/// <summary>
/// Command for user login
/// </summary>
public record LoginCommand : IRequest<LoginCommandResponse>
{
    public string Email { get; init; } = string.Empty;
    public string Password { get; init; } = string.Empty;
    public string? IpAddress { get; init; }
    public string? DeviceInfo { get; init; }
}
