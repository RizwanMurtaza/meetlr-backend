using MediatR;
using Meetlr.Application.Common.Interfaces;
using Meetlr.Application.Interfaces;
using Meetlr.Domain.Entities.Events;
using Meetlr.Domain.Exceptions.DomainErrors.ExceptionByArea;
using Microsoft.EntityFrameworkCore;

namespace Meetlr.Application.Features.MeetlrEvents.Commands.DeleteSingleUseLink;

public class DeleteSingleUseLinkCommandHandler : IRequestHandler<DeleteSingleUseLinkCommand, DeleteSingleUseLinkCommandResponse>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly ICurrentUserService _currentUserService;

    public DeleteSingleUseLinkCommandHandler(
        IUnitOfWork unitOfWork,
        ICurrentUserService currentUserService)
    {
        _unitOfWork = unitOfWork;
        _currentUserService = currentUserService;
    }

    public async Task<DeleteSingleUseLinkCommandResponse> Handle(DeleteSingleUseLinkCommand request, CancellationToken cancellationToken)
    {
        var userId = _currentUserService.UserId!.Value;

        var link = await _unitOfWork.Repository<SingleUseBookingLink>()
            .GetQueryable()
            .FirstOrDefaultAsync(l => l.Id == request.Id && l.UserId == userId && !l.IsDeleted, cancellationToken);

        if (link == null)
        {
            throw SingleUseLinkErrors.LinkNotFound(request.Id);
        }

        // Soft delete by marking as inactive
        link.IsActive = false;
        link.IsDeleted = true;
        link.DeletedAt = DateTime.UtcNow;

        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return new DeleteSingleUseLinkCommandResponse
        {
            Success = true
        };
    }
}
