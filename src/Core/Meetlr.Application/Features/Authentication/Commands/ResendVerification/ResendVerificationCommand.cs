using MediatR;

namespace Meetlr.Application.Features.Authentication.Commands.ResendVerification;

/// <summary>
/// Command for resending verification email
/// </summary>
public record ResendVerificationCommand : IRequest<ResendVerificationCommandResponse>
{
    public string Email { get; init; } = string.Empty;
}
