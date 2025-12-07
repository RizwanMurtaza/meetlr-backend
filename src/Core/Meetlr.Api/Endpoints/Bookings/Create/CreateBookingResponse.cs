using Meetlr.Application.Features.Bookings.Commands.CreateBooking;

namespace Meetlr.Api.Endpoints.Bookings.Create;

public class CreateBookingResponse
{
    public Guid BookingId { get; init; }
    public string ConfirmationToken { get; init; } = string.Empty;
    public DateTime StartTime { get; init; }
    public DateTime EndTime { get; init; }
    public string MeetlrEventName { get; init; } = string.Empty;
    public string HostName { get; init; } = string.Empty;
    public string? Location { get; init; }
    public string? MeetingLink { get; init; }
    public bool RequiresPayment { get; init; }
    public decimal? Amount { get; init; }
    public string? Currency { get; init; }
    public string? PaymentUrl { get; init; }
    public string? PaymentClientSecret { get; init; }

    public static CreateBookingResponse FromCommandResponse(CreateBookingCommandResponse commandResponse)
    {
        return new CreateBookingResponse
        {
            BookingId = commandResponse.BookingId,
            ConfirmationToken = commandResponse.ConfirmationToken,
            StartTime = commandResponse.StartTime,
            EndTime = commandResponse.EndTime,
            MeetlrEventName = commandResponse.MeetlrEventName,
            HostName = commandResponse.HostName,
            Location = commandResponse.Location,
            MeetingLink = commandResponse.MeetingLink,
            RequiresPayment = commandResponse.RequiresPayment,
            Amount = commandResponse.Amount,
            Currency = commandResponse.Currency,
            PaymentUrl = commandResponse.PaymentUrl,
            PaymentClientSecret = commandResponse.PaymentClientSecret
        };
    }
}
