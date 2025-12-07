using FluentValidation;

namespace Meetlr.Module.Analytics.Application.Commands.TrackPageView;

/// <summary>
/// Validator for TrackPageViewCommand
/// </summary>
public class TrackPageViewCommandValidator : AbstractValidator<TrackPageViewCommand>
{
    public TrackPageViewCommandValidator()
    {
        RuleFor(x => x.PageType)
            .NotEmpty()
            .WithMessage("Page type is required")
            .Must(x => x.ToLowerInvariant() is "homepage" or "eventlist" or "eventpage")
            .WithMessage("Page type must be 'homepage', 'eventList', or 'eventPage'");

        RuleFor(x => x.Username)
            .NotEmpty()
            .WithMessage("Username is required")
            .MaximumLength(100)
            .WithMessage("Username cannot exceed 100 characters");

        RuleFor(x => x.SessionId)
            .NotEmpty()
            .WithMessage("Session ID is required")
            .MaximumLength(100)
            .WithMessage("Session ID cannot exceed 100 characters");

        RuleFor(x => x.EventSlug)
            .MaximumLength(200)
            .WithMessage("Event slug cannot exceed 200 characters")
            .When(x => !string.IsNullOrEmpty(x.EventSlug));

        RuleFor(x => x.UserAgent)
            .MaximumLength(500)
            .WithMessage("User agent cannot exceed 500 characters")
            .When(x => !string.IsNullOrEmpty(x.UserAgent));

        RuleFor(x => x.Referrer)
            .MaximumLength(1000)
            .WithMessage("Referrer URL cannot exceed 1000 characters")
            .When(x => !string.IsNullOrEmpty(x.Referrer));
    }
}
