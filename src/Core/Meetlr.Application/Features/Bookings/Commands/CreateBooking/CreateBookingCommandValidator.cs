using FluentValidation;

namespace Meetlr.Application.Features.Bookings.Commands.CreateBooking;

/// <summary>
/// Validator for create booking command
/// </summary>
public class CreateBookingCommandValidator : AbstractValidator<CreateBookingCommand>
{
    public CreateBookingCommandValidator()
    {
        RuleFor(x => x.MeetlrEventId)
            .NotEmpty().WithMessage("Event type ID is required");

        RuleFor(x => x.InviteeName)
            .NotEmpty().WithMessage("Name is required")
            .MaximumLength(200).WithMessage("Name must not exceed 200 characters");

        RuleFor(x => x.InviteeEmail)
            .NotEmpty().WithMessage("Email is required")
            .EmailAddress().WithMessage("Invalid email format");

        RuleFor(x => x.InviteePhone)
            .Matches(@"^\+?[1-9]\d{1,14}$").When(x => !string.IsNullOrWhiteSpace(x.InviteePhone))
            .WithMessage("Invalid phone number format");

        RuleFor(x => x.StartTime)
            .NotEmpty().WithMessage("Start time is required")
            .GreaterThan(DateTime.UtcNow).WithMessage("Start time must be in the future");
    }
}
