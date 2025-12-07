using Meetlr.Domain.Exceptions.Base;
using Meetlr.Domain.Exceptions.Http;

namespace Meetlr.Domain.Exceptions.DomainErrors.ExceptionByArea;

/// <summary>
/// Defines errors related to payment processing.
/// All errors use ApplicationArea.Payments (area code: 12)
/// </summary>
public static class PaymentErrors
{
    private const ApplicationArea Area = ApplicationArea.Payments;

    public static BadRequestException PaymentProviderNotConfigured(string? customMessage = null)
        => new(Area, 1, customMessage ?? "Event does not have a payment provider configured");

    public static NotFoundException PaymentProviderNotAvailable(string providerId, string? customMessage = null)
    {
        var exception = new NotFoundException(Area, 2, customMessage ?? $"Payment provider '{providerId}' is not available");
        exception.WithDetail("ProviderId", providerId);
        return exception;
    }

    public static BadRequestException PaymentAccountNotConnected(string provider, string? customMessage = null)
    {
        var exception = new BadRequestException(Area, 3, customMessage ?? $"Host has not connected their {provider} account for payments");
        exception.WithDetail("Provider", provider);
        return exception;
    }

    public static BadRequestException PaymentAccountChargesDisabled(string provider, string? customMessage = null)
    {
        var exception = new BadRequestException(Area, 4, customMessage ?? $"Host's {provider} account is not enabled for charges");
        exception.WithDetail("Provider", provider);
        return exception;
    }

    public static BadRequestException PaymentIntentCreationFailed(string providerId, string? reason = null, string? customMessage = null)
    {
        var exception = new BadRequestException(Area, 5, customMessage ?? $"Failed to create payment intent with {providerId}");
        exception.WithDetail("ProviderId", providerId);
        if (reason != null)
            exception.WithDetail("Reason", reason);
        return exception;
    }

    public static BadRequestException PaymentConfigurationMissing(string configKey, string? customMessage = null)
    {
        var exception = new BadRequestException(Area, 6, customMessage ?? $"Payment configuration '{configKey}' is missing");
        exception.WithDetail("ConfigKey", configKey);
        return exception;
    }

    public static BadRequestException PaymentProviderDisabled(string providerId, string? customMessage = null)
    {
        var exception = new BadRequestException(Area, 7, customMessage ?? $"{providerId} payment provider is not enabled. Check configuration.");
        exception.WithDetail("ProviderId", providerId);
        return exception;
    }
}
