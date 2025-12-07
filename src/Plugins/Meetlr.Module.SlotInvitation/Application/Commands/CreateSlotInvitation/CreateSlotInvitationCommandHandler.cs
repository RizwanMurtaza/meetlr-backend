using System.Security.Cryptography;
using MediatR;
using Meetlr.Application.Common.Interfaces;
using Meetlr.Application.Interfaces;
using Meetlr.Domain.Entities.Events;
using Meetlr.Domain.Exceptions.DomainErrors.ExceptionByArea;
using Meetlr.Module.SlotInvitation.Domain.Enums;
using Microsoft.EntityFrameworkCore;

namespace Meetlr.Module.SlotInvitation.Application.Commands.CreateSlotInvitation;

public class CreateSlotInvitationCommandHandler : IRequestHandler<CreateSlotInvitationCommand, CreateSlotInvitationCommandResponse>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly ICurrentUserService _currentUserService;
    private readonly INotificationQueueService _notificationQueueService;

    public CreateSlotInvitationCommandHandler(
        IUnitOfWork unitOfWork,
        ICurrentUserService currentUserService,
        INotificationQueueService notificationQueueService)
    {
        _unitOfWork = unitOfWork;
        _currentUserService = currentUserService;
        _notificationQueueService = notificationQueueService;
    }

    public async Task<CreateSlotInvitationCommandResponse> Handle(CreateSlotInvitationCommand request, CancellationToken cancellationToken)
    {
        var userId = _currentUserService.UserId ?? throw AuthenticationErrors.UserNotAuthenticated();

        // Verify the MeetlrEvent exists and belongs to the user
        var meetlrEvent = await _unitOfWork.Repository<MeetlrEvent>().GetQueryable()
            .AsNoTracking()
            .FirstOrDefaultAsync(e => e.Id == request.MeetlrEventId && e.UserId == userId && !e.IsDeleted, cancellationToken);

        if (meetlrEvent == null)
        {
            throw MeetlrEventErrors.MeetlrEventNotFound(request.MeetlrEventId);
        }

        // Get tenant ID from the event
        var tenantId = meetlrEvent.TenantId;

        // Generate secure token
        var token = GenerateSecureToken();

        // Calculate expiration
        var expiresAt = DateTime.UtcNow.AddHours(request.ExpirationHours);

        var slotInvitation = new Domain.Entities.SlotInvitation
        {
            TenantId = tenantId,
            UserId = userId,
            MeetlrEventId = request.MeetlrEventId,
            ContactId = request.ContactId,
            SlotStartTime = request.SlotStartTime,
            SlotEndTime = request.SlotEndTime,
            SpotsReserved = request.SpotsReserved,
            Token = token,
            InviteeEmail = request.InviteeEmail,
            InviteeName = request.InviteeName,
            ExpiresAt = expiresAt,
            ExpirationHours = request.ExpirationHours,
            Status = SlotInvitationStatus.Pending,
            EmailStatus = EmailDeliveryStatus.Queued,
            EmailAttempts = 0
        };

        _unitOfWork.Repository<Domain.Entities.SlotInvitation>().Add(slotInvitation);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        // Queue email notification via notification worker
        await _notificationQueueService.QueueSlotInvitationEmailAsync(
            slotInvitation.Id,
            slotInvitation.MeetlrEventId,
            userId,
            tenantId,
            slotInvitation.InviteeEmail,
            cancellationToken);

        return new CreateSlotInvitationCommandResponse
        {
            Id = slotInvitation.Id,
            MeetlrEventId = slotInvitation.MeetlrEventId,
            ContactId = slotInvitation.ContactId,
            SlotStartTime = slotInvitation.SlotStartTime,
            SlotEndTime = slotInvitation.SlotEndTime,
            SpotsReserved = slotInvitation.SpotsReserved,
            Token = slotInvitation.Token,
            InviteeEmail = slotInvitation.InviteeEmail,
            InviteeName = slotInvitation.InviteeName,
            ExpiresAt = slotInvitation.ExpiresAt,
            ExpirationHours = slotInvitation.ExpirationHours,
            Status = slotInvitation.Status,
            EmailStatus = slotInvitation.EmailStatus,
            CreatedAt = slotInvitation.CreatedAt
        };
    }

    /// <summary>
    /// Generates a cryptographically secure URL-safe token
    /// </summary>
    private static string GenerateSecureToken()
    {
        var bytes = new byte[32];
        using var rng = RandomNumberGenerator.Create();
        rng.GetBytes(bytes);
        // Convert to URL-safe base64
        return Convert.ToBase64String(bytes)
            .Replace("+", "-")
            .Replace("/", "_")
            .TrimEnd('=');
    }
}
