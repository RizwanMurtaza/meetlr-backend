using Meetlr.Application.Features.Authentication.Commands.ForgotPassword;

namespace Meetlr.Api.Endpoints.Auth.ForgotPassword;

/// <summary>
/// Request for initiating password reset
/// </summary>
public record ForgotPasswordRequest
{
    public string Email { get; init; } = string.Empty;

    public static ForgotPasswordCommand ToCommand(ForgotPasswordRequest request)
    {
        return new ForgotPasswordCommand
        {
            Email = request.Email
        };
    }
}
