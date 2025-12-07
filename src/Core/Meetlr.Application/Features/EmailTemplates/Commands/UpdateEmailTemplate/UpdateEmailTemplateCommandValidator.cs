using FluentValidation;

namespace Meetlr.Application.Features.EmailTemplates.Commands.UpdateEmailTemplate;

public class UpdateEmailTemplateCommandValidator : AbstractValidator<UpdateEmailTemplateCommand>
{
    public UpdateEmailTemplateCommandValidator()
    {
        RuleFor(x => x.Id)
            .NotEmpty().WithMessage("Template ID is required");

        RuleFor(x => x.Subject)
            .NotEmpty().WithMessage("Subject is required")
            .MaximumLength(500).WithMessage("Subject cannot exceed 500 characters");

        RuleFor(x => x.HtmlBody)
            .NotEmpty().WithMessage("HTML body is required")
            .MaximumLength(50000).WithMessage("HTML body cannot exceed 50000 characters");

        RuleFor(x => x.PlainTextBody)
            .MaximumLength(10000).WithMessage("Plain text body cannot exceed 10000 characters")
            .When(x => !string.IsNullOrEmpty(x.PlainTextBody));
    }
}
