using Meetlr.Application.Interfaces;
using Microsoft.Extensions.Logging;

namespace Meetlr.Scheduler.Services.Cleanup;

/// <summary>
/// Service for cleaning up expired OTPs older than 24 hours.
/// </summary>
public class OtpCleanupService : IOtpCleanupService
{
    private readonly IOtpService _otpService;
    private readonly ILogger<OtpCleanupService> _logger;

    public OtpCleanupService(
        IOtpService otpService,
        ILogger<OtpCleanupService> logger)
    {
        _otpService = otpService;
        _logger = logger;
    }

    public async Task CleanupExpiredOtpsAsync(CancellationToken cancellationToken)
    {
        try
        {
            _logger.LogInformation("Running OTP cleanup...");

            await _otpService.CleanupExpiredOtpsAsync(cancellationToken);

            _logger.LogInformation("OTP cleanup completed successfully");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error occurred during OTP cleanup");
        }
    }
}
