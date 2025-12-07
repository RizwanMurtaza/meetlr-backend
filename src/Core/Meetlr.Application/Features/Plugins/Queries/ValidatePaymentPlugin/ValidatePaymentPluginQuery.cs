using MediatR;

namespace Meetlr.Application.Features.Plugins.Queries.ValidatePaymentPlugin;

public class ValidatePaymentPluginQuery : IRequest<ValidatePaymentPluginResponse>
{
    public Guid UserId { get; set; }
    public string PaymentProviderType { get; set; } = string.Empty;
}