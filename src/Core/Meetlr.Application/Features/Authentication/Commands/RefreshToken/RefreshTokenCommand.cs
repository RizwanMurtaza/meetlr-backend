using MediatR;

namespace Meetlr.Application.Features.Authentication.Commands.RefreshToken;

public record RefreshTokenCommand : IRequest<RefreshTokenCommandResponse>
{
    public string RefreshToken { get; init; } = string.Empty;
    public string? IpAddress { get; init; }
    public string? DeviceInfo { get; init; }
}
