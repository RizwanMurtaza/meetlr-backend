using MediatR;

namespace Meetlr.Application.Features.Authentication.Commands.VerifyEmail;

/// <summary>
/// Command for verifying user email with OTP
/// </summary>
public record VerifyEmailCommand : IRequest<VerifyEmailCommandResponse>
{
    public string Email { get; init; } = string.Empty;
    public string OtpCode { get; init; } = string.Empty;
}
