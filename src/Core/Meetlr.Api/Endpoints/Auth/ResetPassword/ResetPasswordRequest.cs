using Meetlr.Application.Features.Authentication.Commands.ResetPassword;

namespace Meetlr.Api.Endpoints.Auth.ResetPassword;

/// <summary>
/// Request for resetting password with OTP
/// </summary>
public record ResetPasswordRequest
{
    public string Email { get; init; } = string.Empty;
    public string OtpCode { get; init; } = string.Empty;
    public string NewPassword { get; init; } = string.Empty;

    public static ResetPasswordCommand ToCommand(ResetPasswordRequest request)
    {
        return new ResetPasswordCommand
        {
            Email = request.Email,
            OtpCode = request.OtpCode,
            NewPassword = request.NewPassword
        };
    }
}
