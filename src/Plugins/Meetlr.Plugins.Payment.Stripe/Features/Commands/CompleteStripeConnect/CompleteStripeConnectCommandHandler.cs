using MediatR;
using Meetlr.Application.Interfaces;
using Meetlr.Application.Plugins.Payments;
using Meetlr.Domain.Exceptions.DomainErrors.ExceptionByArea;

namespace Meetlr.Plugins.Payment.Stripe.Features.Commands.CompleteStripeConnect;

public class CompleteStripeConnectCommandHandler : IRequestHandler<CompleteStripeConnectCommand, bool>
{
    private readonly IPaymentProviderFactory _paymentProviderFactory;
    private readonly ICurrentUserService _currentUserService;

    public CompleteStripeConnectCommandHandler(
        IPaymentProviderFactory paymentProviderFactory,
        ICurrentUserService currentUserService)
    {
        _paymentProviderFactory = paymentProviderFactory;
        _currentUserService = currentUserService;
    }

    public async Task<bool> Handle(CompleteStripeConnectCommand request, CancellationToken cancellationToken)
    {
        var userId = _currentUserService.UserId ?? throw AuthenticationErrors.UserNotAuthenticated();

        var plugin = _paymentProviderFactory.GetProvider("stripe")
            ?? throw PluginErrors.PluginNotAvailable("stripe");

        return await plugin.CompleteConnectAsync(request.AuthorizationCode, request.RedirectUri, userId, cancellationToken);
    }
}
