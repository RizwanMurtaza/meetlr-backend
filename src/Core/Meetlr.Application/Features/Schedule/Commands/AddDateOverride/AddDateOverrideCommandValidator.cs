using FluentValidation;

namespace Meetlr.Application.Features.Schedule.Commands.AddDateOverride;

public class AddDateOverrideCommandValidator : AbstractValidator<AddDateOverrideCommand>
{
    public AddDateOverrideCommandValidator()
    {
        RuleFor(x => x.AvailabilityScheduleId)
            .NotEmpty().WithMessage("Availability schedule ID is required");

        RuleFor(x => x.UserId)
            .NotEmpty().WithMessage("User ID is required");

        RuleFor(x => x.Date)
            .NotEmpty().WithMessage("Date is required")
            .Must(date => date.Date >= DateTime.Today).WithMessage("Date cannot be in the past");

        When(x => x.IsAvailable, () =>
        {
            RuleFor(x => x.StartTime)
                .NotNull().WithMessage("Start time is required when available");

            RuleFor(x => x.EndTime)
                .NotNull().WithMessage("End time is required when available");

            RuleFor(x => x)
                .Must(x => x.EndTime > x.StartTime)
                .WithMessage("End time must be after start time")
                .When(x => x.StartTime.HasValue && x.EndTime.HasValue);

            RuleFor(x => x.StartTime)
                .Must(time => time.HasValue && time.Value >= TimeSpan.Zero && time.Value < TimeSpan.FromHours(24))
                .WithMessage("Start time must be between 00:00 and 23:59")
                .When(x => x.StartTime.HasValue);

            RuleFor(x => x.EndTime)
                .Must(time => time.HasValue && time.Value >= TimeSpan.Zero && time.Value <= TimeSpan.FromHours(24))
                .WithMessage("End time must be between 00:00 and 24:00")
                .When(x => x.EndTime.HasValue);
        });
    }
}
