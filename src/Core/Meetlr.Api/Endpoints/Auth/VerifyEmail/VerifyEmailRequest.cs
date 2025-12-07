using Meetlr.Application.Features.Authentication.Commands.VerifyEmail;

namespace Meetlr.Api.Endpoints.Auth.VerifyEmail;

/// <summary>
/// Request for verifying user email with OTP
/// </summary>
public record VerifyEmailRequest
{
    public string Email { get; init; } = string.Empty;
    public string OtpCode { get; init; } = string.Empty;

    public static VerifyEmailCommand ToCommand(VerifyEmailRequest request)
    {
        return new VerifyEmailCommand
        {
            Email = request.Email,
            OtpCode = request.OtpCode
        };
    }
}
