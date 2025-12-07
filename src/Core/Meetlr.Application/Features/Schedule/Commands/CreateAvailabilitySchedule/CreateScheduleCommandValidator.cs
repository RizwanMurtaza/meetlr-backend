using FluentValidation;

namespace Meetlr.Application.Features.Schedule.Commands.CreateAvailabilitySchedule;

public class CreateScheduleCommandValidator : AbstractValidator<CreateScheduleCommandRequest>
{
    public CreateScheduleCommandValidator()
    {
        RuleFor(x => x.UserId)
            .NotEmpty().WithMessage("User ID is required");

        RuleFor(x => x.Name)
            .NotEmpty().WithMessage("Schedule name is required")
            .MaximumLength(200).WithMessage("Schedule name cannot exceed 200 characters");

        RuleFor(x => x.TimeZone)
            .NotEmpty().WithMessage("Time zone is required");

        RuleFor(x => x.MaxBookingsPerSlot)
            .InclusiveBetween(1, 10).WithMessage("Max bookings per slot must be between 1 and 10");

        RuleFor(x => x.WeeklyHours)
            .NotEmpty().WithMessage("At least one weekly hour entry is required")
            .Must(HaveNoOverlappingTimeSlots).WithMessage("Weekly hours cannot have overlapping time slots for the same day");
    }

    private bool HaveNoOverlappingTimeSlots(List<WeeklyHourDto> weeklyHours)
    {
        // Group by day of week
        var groupedByDay = weeklyHours.GroupBy(w => w.DayOfWeek);

        foreach (var dayGroup in groupedByDay)
        {
            var slots = dayGroup.Where(w => w.IsAvailable).OrderBy(w => w.StartTime).ToList();
            
            // Check for overlaps within the same day
            for (int i = 0; i < slots.Count - 1; i++)
            {
                var currentSlot = slots[i];
                var nextSlot = slots[i + 1];

                // If current slot ends after next slot starts, there's an overlap
                if (currentSlot.EndTime > nextSlot.StartTime)
                {
                    return false;
                }
            }
        }

        return true;
    }
}
