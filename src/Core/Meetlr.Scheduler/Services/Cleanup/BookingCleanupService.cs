using Meetlr.Application.Common.Interfaces;
using Meetlr.Domain.Entities.Events;
using Meetlr.Domain.Enums;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace Meetlr.Scheduler.Services.Cleanup;

/// <summary>
/// Service for cleaning up expired pending bookings that were created but never paid.
/// This frees up time slots held for payment.
/// </summary>
public class BookingCleanupService : IBookingCleanupService
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly ILogger<BookingCleanupService> _logger;
    private readonly TimeSpan _expirationThreshold = TimeSpan.FromHours(1);

    public BookingCleanupService(
        IUnitOfWork unitOfWork,
        ILogger<BookingCleanupService> logger)
    {
        _unitOfWork = unitOfWork;
        _logger = logger;
    }

    public async Task CleanupExpiredPendingBookingsAsync(CancellationToken cancellationToken)
    {
        var expirationTime = DateTime.UtcNow.Subtract(_expirationThreshold);

        var expiredBookings = await _unitOfWork.Repository<Booking>()
            .GetQueryable()
            .Where(b =>
                b.Status == BookingStatus.Pending &&
                b.PaymentStatus == PaymentStatus.Pending &&
                b.CreatedAt < expirationTime)
            .ToListAsync(cancellationToken);

        if (!expiredBookings.Any())
        {
            _logger.LogDebug("No expired pending bookings to clean up");
            return;
        }

        _logger.LogInformation("Found {Count} expired pending bookings to cancel", expiredBookings.Count);

        foreach (var booking in expiredBookings)
        {
            try
            {
                booking.Status = BookingStatus.Cancelled;
                booking.PaymentStatus = PaymentStatus.Failed;
                booking.CancelledAt = DateTime.UtcNow;
                booking.CancellationReason = "Payment not completed within the allowed time";
                booking.UpdatedAt = DateTime.UtcNow;

                _unitOfWork.Repository<Booking>().Update(booking);

                _logger.LogInformation(
                    "Cancelled expired pending booking {BookingId} created at {CreatedAt}",
                    booking.Id,
                    booking.CreatedAt);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex,
                    "Failed to cancel expired pending booking {BookingId}",
                    booking.Id);
            }
        }

        await _unitOfWork.SaveChangesAsync(cancellationToken);

        _logger.LogInformation("Successfully cancelled {Count} expired pending bookings", expiredBookings.Count);
    }
}
