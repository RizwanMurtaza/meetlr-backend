using Meetlr.Application.Features.Bookings.Queries.VerifyBookingIdentity;

namespace Meetlr.Api.Endpoints.Bookings.Reschedule;

public class VerifyIdentityRequest
{
    /// <summary>
    /// Confirmation token from the booking email
    /// </summary>
    public string ConfirmationToken { get; init; } = string.Empty;

    /// <summary>
    /// Email or phone number used when creating the booking
    /// </summary>
    public string Identifier { get; init; } = string.Empty;

    public static VerifyBookingIdentityQuery ToQuery(VerifyIdentityRequest request, Guid bookingId)
    {
        return new VerifyBookingIdentityQuery
        {
            BookingId = bookingId,
            ConfirmationToken = request.ConfirmationToken,
            Identifier = request.Identifier
        };
    }
}
