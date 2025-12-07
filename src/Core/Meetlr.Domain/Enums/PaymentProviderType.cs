namespace Meetlr.Domain.Enums;

/// <summary>
/// Types of payment providers supported by the system
/// </summary>
public enum PaymentProviderType
{
    Stripe = 1,
    PayPal = 2,
    Square = 3,
    Razorpay = 4,
    // Add more payment providers as needed
}
