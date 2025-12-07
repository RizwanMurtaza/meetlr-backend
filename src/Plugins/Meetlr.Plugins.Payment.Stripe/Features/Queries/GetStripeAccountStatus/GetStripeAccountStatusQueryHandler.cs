using MediatR;
using Meetlr.Application.Interfaces;
using Meetlr.Application.Plugins.Payments;
using Meetlr.Domain.Exceptions.DomainErrors.ExceptionByArea;

namespace Meetlr.Plugins.Payment.Stripe.Features.Queries.GetStripeAccountStatus;

public class GetStripeAccountStatusQueryHandler : IRequestHandler<GetStripeAccountStatusQuery, StripeAccountStatusResponse?>
{
    private readonly IPaymentProviderFactory _paymentProviderFactory;
    private readonly ICurrentUserService _currentUserService;

    public GetStripeAccountStatusQueryHandler(
        IPaymentProviderFactory paymentProviderFactory,
        ICurrentUserService currentUserService)
    {
        _paymentProviderFactory = paymentProviderFactory;
        _currentUserService = currentUserService;
    }

    public async Task<StripeAccountStatusResponse?> Handle(GetStripeAccountStatusQuery request, CancellationToken cancellationToken)
    {
        var userId = _currentUserService.UserId ?? throw AuthenticationErrors.UserNotAuthenticated();

        var plugin = _paymentProviderFactory.GetProvider("stripe")
            ?? throw PluginErrors.PluginNotAvailable("stripe");

        var status = await plugin.GetConnectionStatusAsync(userId, cancellationToken);

        if (status == null)
            return null;

        // Extract payment-specific fields from Metadata
        var metadata = status.Metadata ?? new Dictionary<string, object>();

        return new StripeAccountStatusResponse
        {
            IsConnected = status.IsConnected,
            ChargesEnabled = metadata.TryGetValue("ChargesEnabled", out var ce) && ce is bool chargesEnabled && chargesEnabled,
            PayoutsEnabled = metadata.TryGetValue("PayoutsEnabled", out var pe) && pe is bool payoutsEnabled && payoutsEnabled,
            DetailsSubmitted = metadata.TryGetValue("DetailsSubmitted", out var ds) && ds is bool detailsSubmitted && detailsSubmitted,
            VerificationStatus = metadata.TryGetValue("VerificationStatus", out var vs) ? vs?.ToString() : null,
            DisabledReason = status.NeedsReconnect ? "Account requires reconnection" : null,
            Country = metadata.TryGetValue("Country", out var country) ? country?.ToString() : null,
            Currency = metadata.TryGetValue("Currency", out var currency) ? currency?.ToString() : null,
            ConnectedAt = status.ConnectedAt
        };
    }
}
