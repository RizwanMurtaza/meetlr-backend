using MediatR;
using Meetlr.Domain.Enums;

namespace Meetlr.Application.Features.Payments.Commands.CreateSeriesPayment;

public class CreateSeriesPaymentCommand : IRequest<CreateSeriesPaymentCommandResponse>
{
    public Guid SeriesId { get; set; }
    public Guid MeetlrEventId { get; set; }
    public List<Guid> BookingIds { get; set; } = new();
    public SeriesPaymentType PaymentType { get; set; }
    public decimal FeePerOccurrence { get; set; }
    public string Currency { get; set; } = "USD";
    public string PaymentProviderType { get; set; } = string.Empty;
    public string? ConnectedAccountId { get; set; }
    public string InviteeEmail { get; set; } = string.Empty;
    public string meetlrEventName { get; set; } = string.Empty;
}
