using FluentValidation;

namespace Meetlr.Application.Features.MeetlrEvents.Commands.Create;

/// <summary>
/// Validator for create event type command
/// </summary>
public class CreateMeetlrEventCommandValidator : AbstractValidator<CreateMeetlrEventCommand>
{
    public CreateMeetlrEventCommandValidator()
    {
        RuleFor(x => x.Name)
            .NotEmpty().WithMessage("Event type name is required")
            .MaximumLength(200).WithMessage("Name must not exceed 200 characters");

        RuleFor(x => x.DurationMinutes)
            .GreaterThan(0).WithMessage("Duration must be greater than 0")
            .LessThanOrEqualTo(1440).WithMessage("Duration cannot exceed 24 hours");

        RuleFor(x => x.BufferTimeBeforeMinutes)
            .GreaterThanOrEqualTo(0).WithMessage("Buffer time cannot be negative")
            .LessThanOrEqualTo(1440).WithMessage("Buffer time cannot exceed 24 hours");

        RuleFor(x => x.BufferTimeAfterMinutes)
            .GreaterThanOrEqualTo(0).WithMessage("Buffer time cannot be negative")
            .LessThanOrEqualTo(1440).WithMessage("Buffer time cannot exceed 24 hours");

        RuleFor(x => x.MinBookingNoticeMinutes)
            .GreaterThanOrEqualTo(0).WithMessage("Minimum booking notice cannot be negative");

        RuleFor(x => x.MaxBookingDaysInFuture)
            .GreaterThan(0).WithMessage("Maximum booking days must be greater than 0")
            .LessThanOrEqualTo(365).WithMessage("Maximum booking days cannot exceed 365");

        RuleFor(x => x.SlugUrl)
            .Matches(@"^[a-z0-9-]+$").When(x => !string.IsNullOrEmpty(x.SlugUrl))
            .WithMessage("Slug can only contain lowercase letters, numbers, and hyphens");

        RuleFor(x => x.Fee)
            .GreaterThan(0).When(x => x.RequiresPayment)
            .WithMessage("Fee must be greater than 0 when payment is required");

        RuleFor(x => x.Currency)
            .NotEmpty().When(x => x.RequiresPayment)
            .WithMessage("Currency is required when payment is required")
            .Length(3).When(x => x.RequiresPayment)
            .WithMessage("Currency must be a 3-letter code (e.g., USD, EUR)");

        RuleFor(x => x.MaxRecurringOccurrences)
            .GreaterThanOrEqualTo(2).When(x => x.AllowsRecurring)
            .WithMessage("Minimum recurring occurrences must be at least 2 when recurring is enabled")
            .LessThanOrEqualTo(10).When(x => x.AllowsRecurring)
            .WithMessage("Maximum recurring occurrences cannot exceed 10");

        RuleFor(x => x.MaxAttendeesPerSlot)
            .GreaterThan(0).When(x => x.MaxAttendeesPerSlot.HasValue)
            .WithMessage("Maximum attendees per slot must be greater than 0")
            .LessThanOrEqualTo(100).When(x => x.MaxAttendeesPerSlot.HasValue)
            .WithMessage("Maximum attendees per slot cannot exceed 100");

        // Location validation
        RuleFor(x => x.LocationDetails)
            .NotEmpty().When(x => x.MeetingLocationType == Meetlr.Domain.Enums.MeetingLocationType.InPerson)
            .WithMessage("Location details are required for in-person meetings")
            .MaximumLength(500).When(x => !string.IsNullOrEmpty(x.LocationDetails))
            .WithMessage("Location details must not exceed 500 characters");
    }
}
