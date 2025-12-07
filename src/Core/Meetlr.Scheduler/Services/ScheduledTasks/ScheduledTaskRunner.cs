using Meetlr.Scheduler.Services.Cleanup;
using Meetlr.Scheduler.Services.Payments;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace Meetlr.Scheduler.Services.ScheduledTasks;

/// <summary>
/// Service responsible for running scheduled background tasks
/// like cleanup, verification, and expiration checks.
/// Uses iteration-based scheduling relative to the polling interval.
/// </summary>
public class ScheduledTaskRunner : IScheduledTaskRunner
{
    private readonly IServiceProvider _serviceProvider;
    private readonly ILogger<ScheduledTaskRunner> _logger;

    // Task intervals (based on 30 second polling)
    private int _bookingCleanupCounter;
    private int _otpCleanupCounter;
    private int _paymentVerificationCounter;
    private int _slotInvitationExpirationCounter;

    private const int BookingCleanupInterval = 20;           // 30s × 20 = 10 minutes
    private const int OtpCleanupInterval = 720;              // 30s × 720 = 6 hours
    private const int PaymentVerificationInterval = 4;       // 30s × 4 = 2 minutes
    private const int SlotInvitationExpirationInterval = 10; // 30s × 10 = 5 minutes

    public ScheduledTaskRunner(
        IServiceProvider serviceProvider,
        ILogger<ScheduledTaskRunner> logger)
    {
        _serviceProvider = serviceProvider;
        _logger = logger;
    }

    public async Task RunScheduledTasksAsync(CancellationToken cancellationToken)
    {
        await RunBookingCleanupAsync(cancellationToken);
        await RunOtpCleanupAsync(cancellationToken);
        await RunPaymentVerificationAsync(cancellationToken);
        await RunSlotInvitationExpirationAsync(cancellationToken);
    }

    private async Task RunBookingCleanupAsync(CancellationToken cancellationToken)
    {
        _bookingCleanupCounter++;
        if (_bookingCleanupCounter < BookingCleanupInterval)
            return;

        _bookingCleanupCounter = 0;

        try
        {
            using var scope = _serviceProvider.CreateScope();
            var service = scope.ServiceProvider.GetRequiredService<IBookingCleanupService>();
            await service.CleanupExpiredPendingBookingsAsync(cancellationToken);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error running booking cleanup task");
        }
    }

    private async Task RunOtpCleanupAsync(CancellationToken cancellationToken)
    {
        _otpCleanupCounter++;
        if (_otpCleanupCounter < OtpCleanupInterval)
            return;

        _otpCleanupCounter = 0;

        try
        {
            using var scope = _serviceProvider.CreateScope();
            var service = scope.ServiceProvider.GetRequiredService<IOtpCleanupService>();
            await service.CleanupExpiredOtpsAsync(cancellationToken);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error running OTP cleanup task");
        }
    }

    private async Task RunPaymentVerificationAsync(CancellationToken cancellationToken)
    {
        _paymentVerificationCounter++;
        if (_paymentVerificationCounter < PaymentVerificationInterval)
            return;

        _paymentVerificationCounter = 0;

        try
        {
            using var scope = _serviceProvider.CreateScope();
            var service = scope.ServiceProvider.GetRequiredService<IPaymentVerificationService>();
            await service.VerifyPendingPaymentsAsync(cancellationToken);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error running payment verification task");
        }
    }

    private async Task RunSlotInvitationExpirationAsync(CancellationToken cancellationToken)
    {
        _slotInvitationExpirationCounter++;
        if (_slotInvitationExpirationCounter < SlotInvitationExpirationInterval)
            return;

        _slotInvitationExpirationCounter = 0;

        try
        {
            using var scope = _serviceProvider.CreateScope();
            var service = scope.ServiceProvider.GetService<ISlotInvitationExpirationService>();
            if (service != null)
            {
                await service.MarkExpiredInvitationsAsync(cancellationToken);
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error running slot invitation expiration task");
        }
    }
}
