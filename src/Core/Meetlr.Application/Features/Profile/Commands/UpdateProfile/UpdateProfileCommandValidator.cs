using System.Text.RegularExpressions;
using FluentValidation;

namespace Meetlr.Application.Features.Profile.Commands.UpdateProfile;

public class UpdateProfileCommandValidator : AbstractValidator<UpdateProfileCommand>
{
    private static readonly Regex PhoneRegex = new(
        @"^[+]?[(]?[0-9]{1,4}[)]?[-\s./0-9]*$",
        RegexOptions.Compiled);

    private static readonly Regex HexColorRegex = new(
        @"^#([A-Fa-f0-9]{6}|[A-Fa-f0-9]{3})$",
        RegexOptions.Compiled);

    public UpdateProfileCommandValidator()
    {
        RuleFor(x => x.UserId)
            .NotEmpty().WithMessage("User ID is required");

        RuleFor(x => x.FirstName)
            .NotEmpty().WithMessage("First name is required")
            .MaximumLength(100).WithMessage("First name cannot exceed 100 characters");

        RuleFor(x => x.LastName)
            .NotEmpty().WithMessage("Last name is required")
            .MaximumLength(100).WithMessage("Last name cannot exceed 100 characters");

        RuleFor(x => x.TimeZone)
            .NotEmpty().WithMessage("Time zone is required")
            .MaximumLength(100).WithMessage("Time zone cannot exceed 100 characters");

        RuleFor(x => x.CompanyName)
            .MaximumLength(100).WithMessage("Company name cannot exceed 100 characters");

        RuleFor(x => x.WelcomeMessage)
            .MaximumLength(500).WithMessage("Welcome message cannot exceed 500 characters");

        RuleFor(x => x.Bio)
            .MaximumLength(500).WithMessage("Bio cannot exceed 500 characters");

        RuleFor(x => x.PhoneNumber)
            .MaximumLength(30).WithMessage("Phone number cannot exceed 30 characters")
            .Must(BeValidPhoneNumber).WithMessage("Please enter a valid phone number")
            .When(x => !string.IsNullOrWhiteSpace(x.PhoneNumber));

        RuleFor(x => x.BrandColor)
            .MaximumLength(7).WithMessage("Brand color must be a valid hex color (e.g., #FF5733)")
            .Must(BeValidHexColor).WithMessage("Please enter a valid hex color (e.g., #FF5733)")
            .When(x => !string.IsNullOrWhiteSpace(x.BrandColor));

        RuleFor(x => x.ProfileImageUrl)
            .MaximumLength(2000).WithMessage("Profile image URL cannot exceed 2000 characters")
            .Must(BeValidUrl).WithMessage("Please enter a valid URL for profile image")
            .When(x => !string.IsNullOrWhiteSpace(x.ProfileImageUrl));

        RuleFor(x => x.Language)
            .MaximumLength(10).WithMessage("Language code cannot exceed 10 characters");

        RuleFor(x => x.DateFormat)
            .MaximumLength(20).WithMessage("Date format cannot exceed 20 characters");

        RuleFor(x => x.TimeFormat)
            .MaximumLength(20).WithMessage("Time format cannot exceed 20 characters");
    }

    private static bool BeValidPhoneNumber(string? phoneNumber)
    {
        if (string.IsNullOrWhiteSpace(phoneNumber))
            return true;

        return PhoneRegex.IsMatch(phoneNumber);
    }

    private static bool BeValidHexColor(string? color)
    {
        if (string.IsNullOrWhiteSpace(color))
            return true;

        return HexColorRegex.IsMatch(color);
    }

    private static bool BeValidUrl(string? url)
    {
        if (string.IsNullOrWhiteSpace(url))
            return true;

        return Uri.TryCreate(url, UriKind.Absolute, out var result)
               && (result.Scheme == Uri.UriSchemeHttp || result.Scheme == Uri.UriSchemeHttps);
    }
}
