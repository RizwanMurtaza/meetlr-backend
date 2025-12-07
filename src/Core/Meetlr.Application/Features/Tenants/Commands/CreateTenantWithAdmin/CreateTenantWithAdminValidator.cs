using FluentValidation;

namespace Meetlr.Application.Features.Tenants.Commands.CreateTenantWithAdmin;

public class CreateTenantWithAdminValidator : AbstractValidator<CreateTenantWithAdminCommand>
{
    public CreateTenantWithAdminValidator()
    {
        RuleFor(x => x.TenantName)
            .NotEmpty().WithMessage("Tenant name is required")
            .MaximumLength(200).WithMessage("Tenant name must not exceed 200 characters");

        RuleFor(x => x.Subdomain)
            .NotEmpty().WithMessage("Subdomain is required")
            .MaximumLength(100).WithMessage("Subdomain must not exceed 100 characters")
            .Matches("^[a-z0-9-]+$").WithMessage("Subdomain can only contain lowercase letters, numbers, and hyphens")
            .Must(subdomain => !subdomain.StartsWith("-") && !subdomain.EndsWith("-"))
                .WithMessage("Subdomain cannot start or end with a hyphen");

        RuleFor(x => x.MainText)
            .NotEmpty().WithMessage("Main text is required")
            .MaximumLength(500).WithMessage("Main text must not exceed 500 characters");

        RuleFor(x => x.FirstName)
            .NotEmpty().WithMessage("First name is required")
            .MaximumLength(100).WithMessage("First name must not exceed 100 characters");

        RuleFor(x => x.LastName)
            .NotEmpty().WithMessage("Last name is required")
            .MaximumLength(100).WithMessage("Last name must not exceed 100 characters");

        RuleFor(x => x.Email)
            .NotEmpty().WithMessage("Email is required")
            .EmailAddress().WithMessage("Invalid email format")
            .MaximumLength(256).WithMessage("Email must not exceed 256 characters");

        RuleFor(x => x.Password)
            .NotEmpty().WithMessage("Password is required")
            .MinimumLength(8).WithMessage("Password must be at least 8 characters")
            .Matches(@"[A-Z]").WithMessage("Password must contain at least one uppercase letter")
            .Matches(@"[a-z]").WithMessage("Password must contain at least one lowercase letter")
            .Matches(@"[0-9]").WithMessage("Password must contain at least one number");

        RuleFor(x => x.TimeZone)
            .NotEmpty().WithMessage("Time zone is required");

        RuleFor(x => x.CustomDomain)
            .MaximumLength(200).WithMessage("Custom domain must not exceed 200 characters")
            .When(x => !string.IsNullOrEmpty(x.CustomDomain));
    }
}
