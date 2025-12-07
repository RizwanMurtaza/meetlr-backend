using MediatR;
using Meetlr.Application.Common.Interfaces;
using Meetlr.Application.Interfaces;
using Meetlr.Domain.Exceptions.DomainErrors.ExceptionByArea;
using Meetlr.Module.SlotInvitation.Domain.Enums;
using Microsoft.EntityFrameworkCore;

namespace Meetlr.Module.SlotInvitation.Application.Queries.GetSlotInvitations;

public class GetSlotInvitationsQueryHandler : IRequestHandler<GetSlotInvitationsQuery, GetSlotInvitationsQueryResponse>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly ICurrentUserService _currentUserService;

    public GetSlotInvitationsQueryHandler(
        IUnitOfWork unitOfWork,
        ICurrentUserService currentUserService)
    {
        _unitOfWork = unitOfWork;
        _currentUserService = currentUserService;
    }

    public async Task<GetSlotInvitationsQueryResponse> Handle(GetSlotInvitationsQuery request, CancellationToken cancellationToken)
    {
        var userId = _currentUserService.UserId ?? throw AuthenticationErrors.UserNotAuthenticated();

        var query = _unitOfWork.Repository<Domain.Entities.SlotInvitation>().GetQueryable()
            .AsNoTracking()
            .Where(si => si.UserId == userId && si.MeetlrEventId == request.MeetlrEventId && !si.IsDeleted);

        // Apply status filter
        if (request.Status.HasValue && Enum.IsDefined(typeof(SlotInvitationStatus), request.Status.Value))
        {
            var status = (SlotInvitationStatus)request.Status.Value;
            query = query.Where(si => si.Status == status);
        }

        // Apply sorting
        query = request.SortBy?.ToLower() switch
        {
            "slotstarttime" => request.SortDescending
                ? query.OrderByDescending(si => si.SlotStartTime)
                : query.OrderBy(si => si.SlotStartTime),
            "expiresat" => request.SortDescending
                ? query.OrderByDescending(si => si.ExpiresAt)
                : query.OrderBy(si => si.ExpiresAt),
            "status" => request.SortDescending
                ? query.OrderByDescending(si => si.Status)
                : query.OrderBy(si => si.Status),
            "inviteeemail" => request.SortDescending
                ? query.OrderByDescending(si => si.InviteeEmail)
                : query.OrderBy(si => si.InviteeEmail),
            "createdat" or _ => request.SortDescending
                ? query.OrderByDescending(si => si.CreatedAt)
                : query.OrderBy(si => si.CreatedAt)
        };

        // Get total count before pagination
        var totalCount = await query.CountAsync(cancellationToken);

        // Apply pagination
        var now = DateTime.UtcNow;
        var invitations = await query
            .Skip((request.PageNumber - 1) * request.PageSize)
            .Take(request.PageSize)
            .Select(si => new SlotInvitationDto
            {
                Id = si.Id,
                MeetlrEventId = si.MeetlrEventId,
                ContactId = si.ContactId,
                SlotStartTime = si.SlotStartTime,
                SlotEndTime = si.SlotEndTime,
                SpotsReserved = si.SpotsReserved,
                Token = si.Token,
                InviteeEmail = si.InviteeEmail,
                InviteeName = si.InviteeName,
                ExpiresAt = si.ExpiresAt,
                ExpirationHours = si.ExpirationHours,
                Status = si.Status,
                BookingId = si.BookingId,
                BookedAt = si.BookedAt,
                EmailStatus = si.EmailStatus,
                EmailAttempts = si.EmailAttempts,
                EmailSentAt = si.EmailSentAt,
                EmailError = si.EmailError,
                CreatedAt = si.CreatedAt,
                UpdatedAt = si.UpdatedAt,
                IsValid = si.Status == SlotInvitationStatus.Pending && si.ExpiresAt > now,
                CanBeDeleted = si.Status != SlotInvitationStatus.Booked,
                CanResendEmail = si.Status == SlotInvitationStatus.Pending && si.EmailAttempts < 3
            })
            .ToListAsync(cancellationToken);

        var totalPages = (int)Math.Ceiling(totalCount / (double)request.PageSize);

        return new GetSlotInvitationsQueryResponse
        {
            Invitations = invitations,
            TotalCount = totalCount,
            PageNumber = request.PageNumber,
            PageSize = request.PageSize,
            TotalPages = totalPages,
            HasPreviousPage = request.PageNumber > 1,
            HasNextPage = request.PageNumber < totalPages
        };
    }
}
