namespace Meetlr.Application.Features.Emails.Commands.SendVerificationEmail;

/// <summary>
/// Response for SendVerificationEmailCommand
/// </summary>
public record SendVerificationEmailCommandResponse
{
    public bool Success { get; init; }
    public string? ErrorMessage { get; init; }
}
