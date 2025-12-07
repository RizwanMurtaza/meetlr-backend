using Meetlr.Application.Common.Interfaces;
using Meetlr.Application.Interfaces;
using Meetlr.Application.Models;
using Meetlr.Module.SlotInvitation.Domain.Enums;
using Microsoft.EntityFrameworkCore;

namespace Meetlr.Module.SlotInvitation.Infrastructure.Services;

/// <summary>
/// Implementation of ISlotInvitationService that provides slot reservation data
/// to the core availability handlers.
/// </summary>
public class SlotInvitationService : ISlotInvitationService
{
    private readonly IUnitOfWork _unitOfWork;

    public SlotInvitationService(IUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork;
    }

    /// <inheritdoc />
    public async Task<List<SlotReservationInfo>> GetActiveReservationsAsync(
        Guid meetlrEventId,
        DateTime startDate,
        DateTime endDate,
        CancellationToken cancellationToken = default)
    {
        var now = DateTime.UtcNow;

        var reservations = await _unitOfWork.Repository<Domain.Entities.SlotInvitation>()
            .GetQueryable()
            .AsNoTracking()
            .Where(si => si.MeetlrEventId == meetlrEventId
                && si.Status == SlotInvitationStatus.Pending
                && si.ExpiresAt > now
                && si.SlotStartTime >= startDate
                && si.SlotStartTime <= endDate)
            .Select(si => new SlotReservationInfo(
                si.SlotStartTime,
                si.SlotEndTime,
                si.SpotsReserved))
            .ToListAsync(cancellationToken);

        return reservations;
    }

    /// <inheritdoc />
    public async Task<SlotInvitationDto?> GetByTokenAsync(
        string token,
        CancellationToken cancellationToken = default)
    {
        var invitation = await _unitOfWork.Repository<Domain.Entities.SlotInvitation>()
            .GetQueryable()
            .AsNoTracking()
            .FirstOrDefaultAsync(si => si.Token == token, cancellationToken);

        if (invitation == null)
            return null;

        return new SlotInvitationDto
        {
            Id = invitation.Id,
            MeetlrEventId = invitation.MeetlrEventId,
            SlotStartTime = invitation.SlotStartTime,
            SlotEndTime = invitation.SlotEndTime,
            SpotsReserved = invitation.SpotsReserved,
            Status = invitation.Status.ToString(),
            ExpiresAt = invitation.ExpiresAt,
            InviteeEmail = invitation.InviteeEmail,
            InviteeName = invitation.InviteeName
        };
    }

    /// <inheritdoc />
    public async Task MarkAsAcceptedAsync(
        Guid slotInvitationId,
        Guid bookingId,
        CancellationToken cancellationToken = default)
    {
        var invitation = await _unitOfWork.Repository<Domain.Entities.SlotInvitation>()
            .GetQueryable()
            .FirstOrDefaultAsync(si => si.Id == slotInvitationId, cancellationToken);

        if (invitation != null)
        {
            invitation.MarkAsBooked(bookingId);
            await _unitOfWork.SaveChangesAsync(cancellationToken);
        }
    }
}
