using FluentValidation;

namespace Meetlr.Application.Features.Schedule.Commands.UpdateSingleDayAvailability;

public class UpdateSingleDayCommandValidator : AbstractValidator<UpdateSingleDayCommandRequest>
{
    public UpdateSingleDayCommandValidator()
    {
        RuleFor(x => x.ScheduleId)
            .NotEmpty()
            .WithMessage("Schedule ID is required");

        RuleFor(x => x.UserId)
            .NotEmpty()
            .WithMessage("User ID is required");

        // When available, must have at least one time slot
        RuleFor(x => x.TimeSlots)
            .NotEmpty()
            .When(x => x.IsAvailable)
            .WithMessage("At least one time slot is required when day is available");

        // Validate each time slot
        RuleForEach(x => x.TimeSlots)
            .ChildRules(slot =>
            {
                slot.RuleFor(s => s.StartTime)
                    .LessThan(s => s.EndTime)
                    .WithMessage("Start time must be before end time");

                slot.RuleFor(s => s.EndTime)
                    .GreaterThan(s => s.StartTime)
                    .WithMessage("End time must be after start time");
            })
            .When(x => x.IsAvailable && x.TimeSlots.Count > 0);
    }
}
