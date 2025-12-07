using MediatR;
using Meetlr.Application.Interfaces;
using Meetlr.Application.Plugins.Payments;
using Meetlr.Domain.Exceptions.DomainErrors.ExceptionByArea;

namespace Meetlr.Plugins.Payment.Stripe.Features.Commands.ConnectStripeAccount;

public class ConnectStripeAccountCommandHandler : IRequestHandler<ConnectStripeAccountCommand, string>
{
    private readonly IPaymentProviderFactory _paymentProviderFactory;
    private readonly ICurrentUserService _currentUserService;

    public ConnectStripeAccountCommandHandler(
        IPaymentProviderFactory paymentProviderFactory,
        ICurrentUserService currentUserService)
    {
        _paymentProviderFactory = paymentProviderFactory;
        _currentUserService = currentUserService;
    }

    public async Task<string> Handle(ConnectStripeAccountCommand request, CancellationToken cancellationToken)
    {
        var userId = _currentUserService.UserId ?? throw AuthenticationErrors.UserNotAuthenticated();

        var plugin = _paymentProviderFactory.GetProvider("stripe")
            ?? throw PluginErrors.PluginNotAvailable("stripe");

        return await plugin.GenerateConnectUrlAsync(userId, request.ReturnUrl, cancellationToken)
            ?? throw PluginErrors.PluginConnectionNotSupported("stripe");
    }
}
