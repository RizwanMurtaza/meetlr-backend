namespace Meetlr.Scheduler.Services.Payments;

/// <summary>
/// Service for verifying pending payments with payment providers.
/// This is a fallback for when webhooks fail to update payment status.
/// </summary>
public interface IPaymentVerificationService
{
    /// <summary>
    /// Verifies pending payments by querying the payment provider directly.
    /// </summary>
    Task VerifyPendingPaymentsAsync(CancellationToken cancellationToken);
}
