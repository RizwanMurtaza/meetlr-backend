using MediatR;
using Meetlr.Application.Common.Interfaces;
using Meetlr.Application.Interfaces;
using Meetlr.Module.Board.Domain.Entities;
using Meetlr.Domain.Exceptions.DomainErrors.ExceptionByArea;
using Microsoft.EntityFrameworkCore;

namespace Meetlr.Module.Board.Application.Commands.UpdateColumn;

public class UpdateColumnCommandHandler : IRequestHandler<UpdateColumnCommand, UpdateColumnCommandResponse>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly ICurrentUserService _currentUserService;

    public UpdateColumnCommandHandler(
        IUnitOfWork unitOfWork,
        ICurrentUserService currentUserService)
    {
        _unitOfWork = unitOfWork;
        _currentUserService = currentUserService;
    }

    public async Task<UpdateColumnCommandResponse> Handle(UpdateColumnCommand request, CancellationToken cancellationToken)
    {
        var userId = _currentUserService.UserId ?? throw AuthenticationErrors.UserNotAuthenticated();

        // Verify board exists and belongs to user
        var board = await _unitOfWork.Repository<Domain.Entities.Board>().GetQueryable()
            .FirstOrDefaultAsync(b => b.Id == request.BoardId && b.UserId == userId && !b.IsDeleted, cancellationToken);

        if (board == null)
        {
            throw BoardErrors.BoardNotFound(request.BoardId);
        }

        var column = await _unitOfWork.Repository<BoardColumn>().GetQueryable()
            .FirstOrDefaultAsync(c => c.Id == request.Id && c.BoardId == request.BoardId && !c.IsDeleted, cancellationToken);

        if (column == null)
        {
            throw BoardErrors.ColumnNotFound(request.Id);
        }

        column.Name = request.Name;

        _unitOfWork.Repository<BoardColumn>().Update(column);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return new UpdateColumnCommandResponse
        {
            Id = column.Id,
            Name = column.Name,
            Position = column.Position
        };
    }
}
