using FluentValidation;

namespace Meetlr.Application.Features.Tenants.Commands.UpdateBranding;

public class UpdateBrandingValidator : AbstractValidator<UpdateBrandingCommand>
{
    public UpdateBrandingValidator()
    {
        RuleFor(x => x.TenantId)
            .NotEmpty().WithMessage("Tenant ID is required");

        RuleFor(x => x.LogoUrl)
            .MaximumLength(500).WithMessage("Logo URL must not exceed 500 characters")
            .When(x => !string.IsNullOrEmpty(x.LogoUrl));

        RuleFor(x => x.PrimaryColor)
            .Matches("^#([A-Fa-f0-9]{6}|[A-Fa-f0-9]{3})$")
                .WithMessage("Primary color must be a valid hex color code")
            .When(x => !string.IsNullOrEmpty(x.PrimaryColor));

        RuleFor(x => x.SecondaryColor)
            .Matches("^#([A-Fa-f0-9]{6}|[A-Fa-f0-9]{3})$")
                .WithMessage("Secondary color must be a valid hex color code")
            .When(x => !string.IsNullOrEmpty(x.SecondaryColor));

        RuleFor(x => x.MainText)
            .MaximumLength(500).WithMessage("Main text must not exceed 500 characters")
            .When(x => !string.IsNullOrEmpty(x.MainText));
    }
}
