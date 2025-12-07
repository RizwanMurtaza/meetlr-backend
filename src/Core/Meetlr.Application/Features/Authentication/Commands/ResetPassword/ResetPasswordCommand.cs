using MediatR;

namespace Meetlr.Application.Features.Authentication.Commands.ResetPassword;

/// <summary>
/// Command for resetting password using OTP code
/// </summary>
public record ResetPasswordCommand : IRequest<ResetPasswordCommandResponse>
{
    public string Email { get; init; } = string.Empty;
    public string OtpCode { get; init; } = string.Empty;
    public string NewPassword { get; init; } = string.Empty;
}
