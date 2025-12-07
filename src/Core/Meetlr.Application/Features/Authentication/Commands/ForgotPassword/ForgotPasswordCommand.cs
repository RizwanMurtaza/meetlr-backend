using MediatR;

namespace Meetlr.Application.Features.Authentication.Commands.ForgotPassword;

/// <summary>
/// Command for initiating password reset flow
/// </summary>
public record ForgotPasswordCommand : IRequest<ForgotPasswordCommandResponse>
{
    public string Email { get; init; } = string.Empty;
}
