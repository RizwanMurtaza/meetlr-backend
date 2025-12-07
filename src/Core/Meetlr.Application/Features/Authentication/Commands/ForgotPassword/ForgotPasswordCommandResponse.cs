namespace Meetlr.Application.Features.Authentication.Commands.ForgotPassword;

/// <summary>
/// Response for forgot password command
/// </summary>
public record ForgotPasswordCommandResponse
{
    public bool Success { get; init; }
    public string Message { get; init; } = string.Empty;
}
