using FluentValidation;

namespace Meetlr.Module.SlotInvitation.Application.Commands.CreateSlotInvitation;

public class CreateSlotInvitationCommandValidator : AbstractValidator<CreateSlotInvitationCommand>
{
    public CreateSlotInvitationCommandValidator()
    {
        RuleFor(x => x.MeetlrEventId)
            .NotEmpty()
            .WithMessage("MeetlrEventId is required");

        RuleFor(x => x.SlotStartTime)
            .NotEmpty()
            .WithMessage("SlotStartTime is required")
            .GreaterThan(DateTime.UtcNow.AddMinutes(-5))
            .WithMessage("SlotStartTime must be in the future");

        RuleFor(x => x.SlotEndTime)
            .NotEmpty()
            .WithMessage("SlotEndTime is required")
            .GreaterThan(x => x.SlotStartTime)
            .WithMessage("SlotEndTime must be after SlotStartTime");

        RuleFor(x => x.SpotsReserved)
            .GreaterThan(0)
            .WithMessage("SpotsReserved must be at least 1")
            .LessThanOrEqualTo(100)
            .WithMessage("SpotsReserved cannot exceed 100");

        RuleFor(x => x.InviteeEmail)
            .NotEmpty()
            .WithMessage("InviteeEmail is required")
            .EmailAddress()
            .WithMessage("InviteeEmail must be a valid email address");

        RuleFor(x => x.InviteeName)
            .MaximumLength(200)
            .WithMessage("InviteeName cannot exceed 200 characters");

        RuleFor(x => x.ExpirationHours)
            .InclusiveBetween(1, 72)
            .WithMessage("ExpirationHours must be between 1 and 72 hours");
    }
}
