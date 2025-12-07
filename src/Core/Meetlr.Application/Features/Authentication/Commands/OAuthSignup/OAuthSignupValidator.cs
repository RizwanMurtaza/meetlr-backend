using FluentValidation;

namespace Meetlr.Application.Features.Authentication.Commands.OAuthSignup;

public class OAuthSignupValidator : AbstractValidator<OAuthSignupCommand>
{
    public OAuthSignupValidator()
    {
        RuleFor(x => x.Email)
            .NotEmpty().WithMessage("Email is required")
            .EmailAddress().WithMessage("Invalid email format")
            .MaximumLength(256).WithMessage("Email must not exceed 256 characters");

        RuleFor(x => x.FirstName)
            .NotEmpty().WithMessage("First name is required")
            .MaximumLength(100).WithMessage("First name must not exceed 100 characters");

        RuleFor(x => x.LastName)
            .NotEmpty().WithMessage("Last name is required")
            .MaximumLength(100).WithMessage("Last name must not exceed 100 characters");

        RuleFor(x => x.Provider)
            .NotEmpty().WithMessage("Provider is required")
            .Must(p => p == "Google" || p == "Microsoft")
                .WithMessage("Provider must be either 'Google' or 'Microsoft'");

        RuleFor(x => x.ProviderId)
            .NotEmpty().WithMessage("Provider ID is required");

        RuleFor(x => x.AccessToken)
            .NotEmpty().WithMessage("Access token is required");

        RuleFor(x => x.TimeZone)
            .NotEmpty().WithMessage("Time zone is required");

        RuleFor(x => x.MeetlrUsername)
            .MaximumLength(100).WithMessage("Username must not exceed 100 characters")
            .Matches("^[a-z0-9-]+$").WithMessage("Username can only contain lowercase letters, numbers, and hyphens")
            .Must(username => username == null || (!username.StartsWith("-") && !username.EndsWith("-")))
                .WithMessage("Username cannot start or end with a hyphen")
            .When(x => !string.IsNullOrEmpty(x.MeetlrUsername));
    }
}
