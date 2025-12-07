using Meetlr.Application.Features.Authentication.Commands.ResendVerification;

namespace Meetlr.Api.Endpoints.Auth.ResendVerification;

/// <summary>
/// Request for resending verification email
/// </summary>
public record ResendVerificationRequest
{
    public string Email { get; init; } = string.Empty;

    public static ResendVerificationCommand ToCommand(ResendVerificationRequest request)
    {
        return new ResendVerificationCommand
        {
            Email = request.Email
        };
    }
}
