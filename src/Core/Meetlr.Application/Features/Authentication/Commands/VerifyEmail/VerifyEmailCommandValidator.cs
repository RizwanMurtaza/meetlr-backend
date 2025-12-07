using FluentValidation;

namespace Meetlr.Application.Features.Authentication.Commands.VerifyEmail;

/// <summary>
/// Validator for verify email command
/// </summary>
public class VerifyEmailCommandValidator : AbstractValidator<VerifyEmailCommand>
{
    public VerifyEmailCommandValidator()
    {
        RuleFor(x => x.Email)
            .NotEmpty().WithMessage("Email is required")
            .EmailAddress().WithMessage("Invalid email format");

        RuleFor(x => x.OtpCode)
            .NotEmpty().WithMessage("OTP code is required")
            .Length(6).WithMessage("OTP code must be 6 digits")
            .Matches(@"^\d{6}$").WithMessage("OTP code must contain only digits");
    }
}
