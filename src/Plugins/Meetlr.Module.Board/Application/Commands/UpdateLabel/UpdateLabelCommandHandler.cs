using MediatR;
using Meetlr.Application.Common.Interfaces;
using Meetlr.Application.Interfaces;
using Meetlr.Module.Board.Domain.Entities;
using Meetlr.Domain.Exceptions.DomainErrors.ExceptionByArea;
using Microsoft.EntityFrameworkCore;

namespace Meetlr.Module.Board.Application.Commands.UpdateLabel;

public class UpdateLabelCommandHandler : IRequestHandler<UpdateLabelCommand, UpdateLabelCommandResponse>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly ICurrentUserService _currentUserService;

    public UpdateLabelCommandHandler(
        IUnitOfWork unitOfWork,
        ICurrentUserService currentUserService)
    {
        _unitOfWork = unitOfWork;
        _currentUserService = currentUserService;
    }

    public async Task<UpdateLabelCommandResponse> Handle(UpdateLabelCommand request, CancellationToken cancellationToken)
    {
        var userId = _currentUserService.UserId ?? throw AuthenticationErrors.UserNotAuthenticated();

        // Verify board exists and belongs to user
        var board = await _unitOfWork.Repository<Domain.Entities.Board>().GetQueryable()
            .FirstOrDefaultAsync(b => b.Id == request.BoardId && b.UserId == userId && !b.IsDeleted, cancellationToken);

        if (board == null)
        {
            throw BoardErrors.BoardNotFound(request.BoardId);
        }

        var label = await _unitOfWork.Repository<BoardLabel>().GetQueryable()
            .FirstOrDefaultAsync(l => l.Id == request.Id && l.BoardId == request.BoardId && !l.IsDeleted, cancellationToken);

        if (label == null)
        {
            throw BoardErrors.LabelNotFound(request.Id);
        }

        if (!string.IsNullOrWhiteSpace(request.Name))
            label.Name = request.Name;

        if (!string.IsNullOrWhiteSpace(request.Color))
            label.Color = request.Color;

        _unitOfWork.Repository<BoardLabel>().Update(label);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return new UpdateLabelCommandResponse
        {
            Id = label.Id,
            Name = label.Name,
            Color = label.Color
        };
    }
}
