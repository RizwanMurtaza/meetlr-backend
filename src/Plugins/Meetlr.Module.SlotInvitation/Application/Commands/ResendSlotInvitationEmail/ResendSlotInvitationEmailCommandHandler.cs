using MediatR;
using Meetlr.Application.Common.Interfaces;
using Meetlr.Application.Interfaces;
using Meetlr.Domain.Exceptions.DomainErrors.ExceptionByArea;
using Meetlr.Module.SlotInvitation.Domain.Enums;
using Microsoft.EntityFrameworkCore;

namespace Meetlr.Module.SlotInvitation.Application.Commands.ResendSlotInvitationEmail;

public class ResendSlotInvitationEmailCommandHandler : IRequestHandler<ResendSlotInvitationEmailCommand, ResendSlotInvitationEmailCommandResponse>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly ICurrentUserService _currentUserService;
    private readonly INotificationQueueService _notificationQueueService;

    public ResendSlotInvitationEmailCommandHandler(
        IUnitOfWork unitOfWork,
        ICurrentUserService currentUserService,
        INotificationQueueService notificationQueueService)
    {
        _unitOfWork = unitOfWork;
        _currentUserService = currentUserService;
        _notificationQueueService = notificationQueueService;
    }

    public async Task<ResendSlotInvitationEmailCommandResponse> Handle(
        ResendSlotInvitationEmailCommand request,
        CancellationToken cancellationToken)
    {
        var userId = _currentUserService.UserId ?? throw AuthenticationErrors.UserNotAuthenticated();

        // Find the invitation
        var invitation = await _unitOfWork.Repository<Domain.Entities.SlotInvitation>()
            .GetQueryable()
            .FirstOrDefaultAsync(
                si => si.Id == request.SlotInvitationId && si.UserId == userId,
                cancellationToken);

        if (invitation == null)
        {
            return new ResendSlotInvitationEmailCommandResponse
            {
                Success = false,
                Message = "Slot invitation not found",
                EmailAttempts = 0,
                CanResendAgain = false
            };
        }

        // Check if invitation is still pending
        if (invitation.Status != SlotInvitationStatus.Pending)
        {
            return new ResendSlotInvitationEmailCommandResponse
            {
                Success = false,
                Message = $"Cannot resend email for invitation with status: {invitation.Status}",
                EmailAttempts = invitation.EmailAttempts,
                CanResendAgain = false
            };
        }

        // Check if invitation has expired
        if (invitation.ExpiresAt <= DateTime.UtcNow)
        {
            return new ResendSlotInvitationEmailCommandResponse
            {
                Success = false,
                Message = "Cannot resend email for expired invitation",
                EmailAttempts = invitation.EmailAttempts,
                CanResendAgain = false
            };
        }

        // Check if we can resend (max 3 attempts)
        if (!invitation.CanResendEmail)
        {
            return new ResendSlotInvitationEmailCommandResponse
            {
                Success = false,
                Message = "Maximum email attempts (3) reached for this invitation",
                EmailAttempts = invitation.EmailAttempts,
                CanResendAgain = false
            };
        }

        // Reset email status to queued for resend
        invitation.EmailStatus = EmailDeliveryStatus.Queued;

        await _unitOfWork.SaveChangesAsync(cancellationToken);

        // Queue the email notification again
        await _notificationQueueService.QueueSlotInvitationEmailAsync(
            invitation.Id,
            invitation.MeetlrEventId,
            userId,
            invitation.TenantId,
            invitation.InviteeEmail,
            cancellationToken);

        return new ResendSlotInvitationEmailCommandResponse
        {
            Success = true,
            Message = "Email queued for resend",
            EmailAttempts = invitation.EmailAttempts + 1, // Will be incremented when sent
            CanResendAgain = invitation.EmailAttempts + 1 < 3
        };
    }
}
