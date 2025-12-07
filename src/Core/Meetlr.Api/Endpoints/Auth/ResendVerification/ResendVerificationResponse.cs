using Meetlr.Application.Features.Authentication.Commands.ResendVerification;

namespace Meetlr.Api.Endpoints.Auth.ResendVerification;

/// <summary>
/// Response for resend verification
/// </summary>
public record ResendVerificationResponse
{
    public bool Success { get; init; }
    public string Message { get; init; } = string.Empty;

    public static ResendVerificationResponse FromCommandResponse(ResendVerificationCommandResponse response)
    {
        return new ResendVerificationResponse
        {
            Success = response.Success,
            Message = response.Message
        };
    }
}
