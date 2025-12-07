using Meetlr.Application.Features.Authentication.Commands.RefreshToken;

namespace Meetlr.Api.Endpoints.Auth.RefreshToken;

public record RefreshTokenRequest
{
    public string RefreshToken { get; init; } = string.Empty;

    public static RefreshTokenCommand ToCommand(RefreshTokenRequest request, string? ipAddress, string? deviceInfo)
    {
        return new RefreshTokenCommand
        {
            RefreshToken = request.RefreshToken,
            IpAddress = ipAddress,
            DeviceInfo = deviceInfo
        };
    }
}
