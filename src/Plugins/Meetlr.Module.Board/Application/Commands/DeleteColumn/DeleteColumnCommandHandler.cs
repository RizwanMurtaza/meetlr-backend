using MediatR;
using Meetlr.Application.Common.Interfaces;
using Meetlr.Application.Interfaces;
using Meetlr.Module.Board.Domain.Entities;
using Meetlr.Domain.Exceptions.DomainErrors.ExceptionByArea;
using Microsoft.EntityFrameworkCore;

namespace Meetlr.Module.Board.Application.Commands.DeleteColumn;

public class DeleteColumnCommandHandler : IRequestHandler<DeleteColumnCommand, DeleteColumnCommandResponse>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly ICurrentUserService _currentUserService;

    public DeleteColumnCommandHandler(
        IUnitOfWork unitOfWork,
        ICurrentUserService currentUserService)
    {
        _unitOfWork = unitOfWork;
        _currentUserService = currentUserService;
    }

    public async Task<DeleteColumnCommandResponse> Handle(DeleteColumnCommand request, CancellationToken cancellationToken)
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
            .Include(c => c.Tasks)
            .FirstOrDefaultAsync(c => c.Id == request.Id && c.BoardId == request.BoardId && !c.IsDeleted, cancellationToken);

        if (column == null)
        {
            throw BoardErrors.ColumnNotFound(request.Id);
        }

        // Soft delete the column (and its tasks via cascade)
        _unitOfWork.Repository<BoardColumn>().Delete(column);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return new DeleteColumnCommandResponse
        {
            Success = true,
            Id = request.Id
        };
    }
}
