using Meetlr.Application.Features.Authentication.Commands.Login;

namespace Meetlr.Api.Endpoints.Auth.Login;

public record LoginRequest
{
    public string Email { get; init; } = string.Empty;
    public string Password { get; init; } = string.Empty;

    public static LoginCommand ToCommand(LoginRequest request, string? ipAddress = null, string? deviceInfo = null)
    {
        return new LoginCommand
        {
            Email = request.Email,
            Password = request.Password,
            IpAddress = ipAddress,
            DeviceInfo = deviceInfo
        };
    }
}
