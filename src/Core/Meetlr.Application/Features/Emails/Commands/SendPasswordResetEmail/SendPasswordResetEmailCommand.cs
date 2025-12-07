using MediatR;

namespace Meetlr.Application.Features.Emails.Commands.SendPasswordResetEmail;

/// <summary>
/// Command to send password reset email to user
/// Handler is in Meetlr.Module.Notifications
/// </summary>
public record SendPasswordResetEmailCommand : IRequest<SendPasswordResetEmailCommandResponse>
{
    public Guid UserId { get; init; }
    public string Email { get; init; } = string.Empty;
    public string ResetToken { get; init; } = string.Empty;
}
