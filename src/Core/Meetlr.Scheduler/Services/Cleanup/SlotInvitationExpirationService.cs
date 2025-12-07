using Meetlr.Application.Common.Interfaces;
using Meetlr.Module.SlotInvitation.Domain.Enums;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace Meetlr.Scheduler.Services.Cleanup;

/// <summary>
/// Service for marking expired slot invitations as expired.
/// This frees up reserved time slots for other bookings.
/// </summary>
public class SlotInvitationExpirationService : ISlotInvitationExpirationService
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly ILogger<SlotInvitationExpirationService> _logger;

    public SlotInvitationExpirationService(
        IUnitOfWork unitOfWork,
        ILogger<SlotInvitationExpirationService> logger)
    {
        _unitOfWork = unitOfWork;
        _logger = logger;
    }

    public async Task MarkExpiredInvitationsAsync(CancellationToken cancellationToken)
    {
        var now = DateTime.UtcNow;

        // Find all pending invitations that have expired
        var expiredInvitations = await _unitOfWork.Repository<Meetlr.Module.SlotInvitation.Domain.Entities.SlotInvitation>()
            .GetQueryable()
            .Where(si =>
                si.Status == SlotInvitationStatus.Pending &&
                si.ExpiresAt <= now)
            .ToListAsync(cancellationToken);

        if (!expiredInvitations.Any())
        {
            _logger.LogDebug("No expired slot invitations to process");
            return;
        }

        _logger.LogInformation("Found {Count} expired slot invitations to mark as expired", expiredInvitations.Count);

        foreach (var invitation in expiredInvitations)
        {
            try
            {
                invitation.MarkAsExpired();

                _logger.LogInformation(
                    "Marked slot invitation {InvitationId} as expired (expired at {ExpiresAt})",
                    invitation.Id,
                    invitation.ExpiresAt);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex,
                    "Failed to mark slot invitation {InvitationId} as expired",
                    invitation.Id);
            }
        }

        await _unitOfWork.SaveChangesAsync(cancellationToken);

        _logger.LogInformation("Successfully marked {Count} slot invitations as expired", expiredInvitations.Count);
    }
}
