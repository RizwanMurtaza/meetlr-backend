namespace Meetlr.Application.Features.Authentication.Commands.ResetPassword;

/// <summary>
/// Response for reset password command
/// </summary>
public record ResetPasswordCommandResponse
{
    public bool Success { get; init; }
    public string Message { get; init; } = string.Empty;
    public string? Token { get; init; } // Optional: Auto-login after reset
}
