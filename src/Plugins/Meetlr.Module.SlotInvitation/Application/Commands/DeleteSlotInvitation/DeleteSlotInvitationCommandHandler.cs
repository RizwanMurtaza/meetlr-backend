using MediatR;
using Meetlr.Application.Common.Interfaces;
using Meetlr.Application.Interfaces;
using Meetlr.Domain.Exceptions.DomainErrors.ExceptionByArea;
using Microsoft.EntityFrameworkCore;

namespace Meetlr.Module.SlotInvitation.Application.Commands.DeleteSlotInvitation;

public class DeleteSlotInvitationCommandHandler : IRequestHandler<DeleteSlotInvitationCommand, DeleteSlotInvitationCommandResponse>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly ICurrentUserService _currentUserService;

    public DeleteSlotInvitationCommandHandler(
        IUnitOfWork unitOfWork,
        ICurrentUserService currentUserService)
    {
        _unitOfWork = unitOfWork;
        _currentUserService = currentUserService;
    }

    public async Task<DeleteSlotInvitationCommandResponse> Handle(DeleteSlotInvitationCommand request, CancellationToken cancellationToken)
    {
        var userId = _currentUserService.UserId ?? throw AuthenticationErrors.UserNotAuthenticated();

        var invitation = await _unitOfWork.Repository<Domain.Entities.SlotInvitation>().GetQueryable()
            .FirstOrDefaultAsync(si => si.Id == request.Id && si.UserId == userId && !si.IsDeleted, cancellationToken);

        if (invitation == null)
        {
            throw SlotInvitationErrors.InvitationNotFound(request.Id);
        }

        // Cannot delete if already booked
        if (!invitation.CanBeDeleted)
        {
            throw SlotInvitationErrors.CannotDeleteBookedInvitation(request.Id);
        }

        // Mark as cancelled instead of hard delete
        invitation.MarkAsCancelled();

        // Also soft delete for consistency
        _unitOfWork.Repository<Domain.Entities.SlotInvitation>().Delete(invitation);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return new DeleteSlotInvitationCommandResponse
        {
            Success = true,
            Id = request.Id
        };
    }
}
