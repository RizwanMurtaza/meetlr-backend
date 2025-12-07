using MediatR;

namespace Meetlr.Application.Features.Bookings.Queries.VerifyBookingIdentity;

public class VerifyBookingIdentityQuery : IRequest<VerifyBookingIdentityQueryResponse>
{
    public Guid BookingId { get; set; }
    public string ConfirmationToken { get; set; } = string.Empty;
    public string Identifier { get; set; } = string.Empty; // Email or phone number
}
