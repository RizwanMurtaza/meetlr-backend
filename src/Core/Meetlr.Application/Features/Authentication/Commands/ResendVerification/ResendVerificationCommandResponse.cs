namespace Meetlr.Application.Features.Authentication.Commands.ResendVerification;

/// <summary>
/// Response for resend verification command
/// </summary>
public record ResendVerificationCommandResponse
{
    public bool Success { get; init; }
    public string Message { get; init; } = string.Empty;
}
