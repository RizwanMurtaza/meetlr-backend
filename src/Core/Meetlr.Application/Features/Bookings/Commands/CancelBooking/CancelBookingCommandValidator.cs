using FluentValidation;

namespace Meetlr.Application.Features.Bookings.Commands.CancelBooking;

/// <summary>
/// Validator for cancel booking command
/// </summary>
public class CancelBookingCommandValidator : AbstractValidator<CancelBookingCommand>
{
    public CancelBookingCommandValidator()
    {
        RuleFor(x => x.BookingId)
            .NotEmpty().WithMessage("Booking ID is required");
    }
}
