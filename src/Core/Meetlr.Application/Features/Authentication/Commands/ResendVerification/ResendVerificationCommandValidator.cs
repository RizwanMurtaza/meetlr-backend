using FluentValidation;

namespace Meetlr.Application.Features.Authentication.Commands.ResendVerification;

/// <summary>
/// Validator for resend verification command
/// </summary>
public class ResendVerificationCommandValidator : AbstractValidator<ResendVerificationCommand>
{
    public ResendVerificationCommandValidator()
    {
        RuleFor(x => x.Email)
            .NotEmpty().WithMessage("Email is required")
            .EmailAddress().WithMessage("Invalid email format");
    }
}
