using FluentValidation;

namespace Meetlr.Application.Features.UserSettings.Commands.UpdateUserSettings;

public class UpdateUserSettingsCommandValidator : AbstractValidator<UpdateUserSettingsCommand>
{
    // List of valid ISO 4217 currency codes
    private static readonly HashSet<string> ValidCurrencyCodes = new()
    {
        "USD", "GBP", "EUR", "CAD", "AUD", "JPY", "CHF", "CNY", "INR", "NZD",
        "SGD", "HKD", "NOK", "SEK", "DKK", "PLN", "CZK", "HUF", "RON", "BGN",
        "TRY", "BRL", "MXN", "ZAR", "KRW", "TWD", "THB", "MYR", "IDR", "PHP",
        "RUB", "AED", "SAR", "QAR", "KWD", "BHD", "OMR", "ILS", "EGP", "PKR"
    };

    public UpdateUserSettingsCommandValidator()
    {
        RuleFor(x => x.UserId)
            .NotEmpty().WithMessage("User ID is required");

        RuleFor(x => x.DefaultCurrency)
            .NotEmpty().WithMessage("Default currency is required")
            .Length(3).WithMessage("Currency code must be exactly 3 characters")
            .Must(BeValidCurrencyCode).WithMessage("Currency code must be a valid ISO 4217 code");

        RuleFor(x => x.DefaultEventDuration)
            .InclusiveBetween(15, 480)
            .WithMessage("Default event duration must be between 15 and 480 minutes");

        RuleFor(x => x.DefaultBufferBefore)
            .InclusiveBetween(0, 120)
            .WithMessage("Default buffer before must be between 0 and 120 minutes");

        RuleFor(x => x.DefaultBufferAfter)
            .InclusiveBetween(0, 120)
            .WithMessage("Default buffer after must be between 0 and 120 minutes");

        RuleFor(x => x.DefaultMinBookingNotice)
            .InclusiveBetween(0, 10080)
            .WithMessage("Default minimum booking notice must be between 0 and 10080 minutes (1 week)");

        RuleFor(x => x.DefaultReminderHours)
            .InclusiveBetween(1, 168)
            .WithMessage("Default reminder hours must be between 1 and 168 hours (1 week)");

        RuleFor(x => x.DefaultLocationDetails)
            .MaximumLength(500)
            .WithMessage("Default location details cannot exceed 500 characters")
            .When(x => !string.IsNullOrEmpty(x.DefaultLocationDetails));

        RuleFor(x => x.BookingPageTheme)
            .MaximumLength(50)
            .WithMessage("Booking page theme cannot exceed 50 characters")
            .When(x => !string.IsNullOrEmpty(x.BookingPageTheme));

        RuleFor(x => x.JobTitle)
            .MaximumLength(100)
            .WithMessage("Job title cannot exceed 100 characters")
            .When(x => !string.IsNullOrEmpty(x.JobTitle));

        RuleFor(x => x.WebsiteUrl)
            .MaximumLength(500)
            .WithMessage("Website URL cannot exceed 500 characters")
            .Must(BeValidUrlOrEmpty)
            .WithMessage("Website URL must be a valid HTTP or HTTPS URL")
            .When(x => !string.IsNullOrEmpty(x.WebsiteUrl));

        RuleFor(x => x.LinkedInUrl)
            .MaximumLength(500)
            .WithMessage("LinkedIn URL cannot exceed 500 characters")
            .Must(BeValidUrlOrEmpty)
            .WithMessage("LinkedIn URL must be a valid HTTP or HTTPS URL")
            .When(x => !string.IsNullOrEmpty(x.LinkedInUrl));

        RuleFor(x => x.TwitterUrl)
            .MaximumLength(500)
            .WithMessage("Twitter URL cannot exceed 500 characters")
            .Must(BeValidUrlOrEmpty)
            .WithMessage("Twitter URL must be a valid HTTP or HTTPS URL")
            .When(x => !string.IsNullOrEmpty(x.TwitterUrl));

        RuleFor(x => x.DefaultMeetingLocationType)
            .IsInEnum()
            .WithMessage("Default meeting location type must be a valid value");

        RuleFor(x => x.WeekStartsOn)
            .IsInEnum()
            .WithMessage("Week starts on must be a valid value");
    }

    private bool BeValidCurrencyCode(string currencyCode)
    {
        return ValidCurrencyCodes.Contains(currencyCode.ToUpperInvariant());
    }

    private bool BeValidUrlOrEmpty(string? url)
    {
        if (string.IsNullOrWhiteSpace(url))
            return true;

        return Uri.TryCreate(url, UriKind.Absolute, out var uriResult)
            && (uriResult.Scheme == Uri.UriSchemeHttp || uriResult.Scheme == Uri.UriSchemeHttps);
    }
}
