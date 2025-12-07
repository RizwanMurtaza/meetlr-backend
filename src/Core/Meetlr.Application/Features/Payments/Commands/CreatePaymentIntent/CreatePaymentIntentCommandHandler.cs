using MediatR;
using Meetlr.Application.Common.Interfaces;
using Meetlr.Application.Plugins.Payments;
using Meetlr.Domain.Exceptions.DomainErrors.ExceptionByArea;
using Meetlr.Domain.Exceptions.Http;
using Microsoft.Extensions.Logging;

namespace Meetlr.Application.Features.Payments.Commands.CreatePaymentIntent;

/// <summary>
/// Handler for creating payment intents - follows CQRS pattern
/// </summary>
public class CreatePaymentIntentCommandHandler : IRequestHandler<CreatePaymentIntentCommand, CreatePaymentIntentCommandResponse>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IPaymentProviderFactory _paymentProviderFactory;
    private readonly ILogger<CreatePaymentIntentCommandHandler> _logger;

    public CreatePaymentIntentCommandHandler(
        IUnitOfWork unitOfWork,
        IPaymentProviderFactory paymentProviderFactory,
        ILogger<CreatePaymentIntentCommandHandler> logger)
    {
        _unitOfWork = unitOfWork;
        _paymentProviderFactory = paymentProviderFactory;
        _logger = logger;
    }

    public async Task<CreatePaymentIntentCommandResponse> Handle(
        CreatePaymentIntentCommand request,
        CancellationToken cancellationToken)
    {
        try
        {
            // Validate payment provider
            if (string.IsNullOrEmpty(request.PaymentProviderType))
            {
                throw PaymentErrors.PaymentProviderNotConfigured();
            }

            var plugin = _paymentProviderFactory.GetProvider(request.PaymentProviderType)
                ?? throw PaymentErrors.PaymentProviderNotAvailable(request.PaymentProviderType);

            // Get connected account ID from the plugin (provider-specific lookup)
            var connectedAccountId = await plugin.GetConnectedAccountIdAsync(request.UserId, cancellationToken);
            if (string.IsNullOrEmpty(connectedAccountId))
            {
                _logger.LogWarning(
                    "User {UserId} does not have a connected {Provider} account",
                    request.UserId, request.PaymentProviderType);
                throw PaymentErrors.PaymentAccountNotConnected(request.PaymentProviderType);
            }

            // Create payment intent
            var description = $"Booking payment - {request.BookingId}";
            var paymentIntentResult = await plugin.CreatePaymentIntentAsync(
                request.BookingId,
                request.Amount,
                request.Currency,
                connectedAccountId,
                description,
                request.Metadata ?? new Dictionary<string, string>(),
                cancellationToken);

            _logger.LogInformation(
                "Payment intent created successfully. BookingId: {BookingId}, PaymentIntentId: {PaymentIntentId}",
                request.BookingId,
                paymentIntentResult.PaymentIntentId);

            return new CreatePaymentIntentCommandResponse
            {
                Success = true,
                PaymentIntentId = paymentIntentResult.PaymentIntentId,
                ClientSecret = paymentIntentResult.ClientSecret,
                PaymentUrl = $"/payment/{request.BookingId}"
            };
        }
        catch (Exception ex) when (ex is not BadRequestException)
        {
            _logger.LogError(ex, "Failed to create payment intent for booking {BookingId}", request.BookingId);

            return new CreatePaymentIntentCommandResponse
            {
                Success = false,
                ErrorMessage = ex.Message
            };
        }
    }
}
