namespace Meetlr.Application.Features.Emails.Commands.SendPasswordResetEmail;

/// <summary>
/// Response for SendPasswordResetEmailCommand
/// </summary>
public record SendPasswordResetEmailCommandResponse
{
    public bool Success { get; init; }
    public string? ErrorMessage { get; init; }
}
