using MediatR;

namespace Meetlr.Application.Features.Emails.Commands.SendVerificationEmail;

/// <summary>
/// Command to send email verification code to user
/// Handler is in Meetlr.Module.Notifications
/// </summary>
public record SendVerificationEmailCommand : IRequest<SendVerificationEmailCommandResponse>
{
    public Guid UserId { get; init; }
    public string Email { get; init; } = string.Empty;
    public string VerificationCode { get; init; } = string.Empty;
}
