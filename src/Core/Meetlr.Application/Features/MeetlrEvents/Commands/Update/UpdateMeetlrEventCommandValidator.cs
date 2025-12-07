using FluentValidation;

namespace Meetlr.Application.Features.MeetlrEvents.Commands.Update;

public class UpdateMeetlrEventCommandValidator : AbstractValidator<UpdateMeetlrEventCommand>
{
    public UpdateMeetlrEventCommandValidator()
    {
        RuleFor(x => x.Id)
            .NotEmpty().WithMessage("Event type ID is required");

        RuleFor(x => x.UserId)
            .NotEmpty().WithMessage("User ID is required");

        // Only validate name if provided
        When(x => x.Name != null, () =>
        {
            RuleFor(x => x.Name)
                .NotEmpty().WithMessage("Event type name cannot be empty")
                .MaximumLength(200).WithMessage("Event type name cannot exceed 200 characters");
        });

        // Color validation if provided
        When(x => x.Color != null, () =>
        {
            RuleFor(x => x.Color)
                .MaximumLength(20).WithMessage("Color must not exceed 20 characters");
        });
    }
}
