using MediatR;
using Meetlr.Application.Common.Interfaces;
using Meetlr.Domain.Entities.Events;
using Meetlr.Module.SlotInvitation.Domain.Enums;
using Microsoft.EntityFrameworkCore;

namespace Meetlr.Module.SlotInvitation.Application.Queries.GetSlotInvitationByToken;

public class GetSlotInvitationByTokenQueryHandler : IRequestHandler<GetSlotInvitationByTokenQuery, GetSlotInvitationByTokenQueryResponse?>
{
    private readonly IUnitOfWork _unitOfWork;

    public GetSlotInvitationByTokenQueryHandler(IUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork;
    }

    public async Task<GetSlotInvitationByTokenQueryResponse?> Handle(
        GetSlotInvitationByTokenQuery request,
        CancellationToken cancellationToken)
    {
        // Find the invitation by token
        var invitation = await _unitOfWork.Repository<Domain.Entities.SlotInvitation>()
            .GetQueryable()
            .AsNoTracking()
            .FirstOrDefaultAsync(si => si.Token == request.Token, cancellationToken);

        if (invitation == null)
            return null;

        // Get the event details
        var meetlrEvent = await _unitOfWork.Repository<MeetlrEvent>()
            .GetQueryable()
            .AsNoTracking()
            .Include(e => e.User)
            .FirstOrDefaultAsync(e => e.Id == invitation.MeetlrEventId, cancellationToken);

        if (meetlrEvent == null)
            return null;

        var hostFullName = $"{meetlrEvent.User.FirstName} {meetlrEvent.User.LastName}".Trim();
        if (string.IsNullOrEmpty(hostFullName))
        {
            hostFullName = meetlrEvent.User.Email ?? "Host";
        }

        return new GetSlotInvitationByTokenQueryResponse
        {
            Id = invitation.Id,
            TenantId = invitation.TenantId,
            MeetlrEventId = invitation.MeetlrEventId,
            MeetlrEventName = meetlrEvent.Name,
            MeetlrEventSlug = meetlrEvent.Slug,
            HostName = hostFullName,
            HostUsername = meetlrEvent.User.MeetlrUsername ?? meetlrEvent.UserId.ToString(),
            SlotStartTime = invitation.SlotStartTime,
            SlotEndTime = invitation.SlotEndTime,
            DurationMinutes = meetlrEvent.DurationMinutes,
            SpotsReserved = invitation.SpotsReserved,
            InviteeEmail = invitation.InviteeEmail,
            InviteeName = invitation.InviteeName,
            ExpiresAt = invitation.ExpiresAt,
            Status = invitation.Status.ToString(),
            IsValid = invitation.IsValid
        };
    }
}
