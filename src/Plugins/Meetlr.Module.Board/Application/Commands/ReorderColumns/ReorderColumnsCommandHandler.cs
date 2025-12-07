using MediatR;
using Meetlr.Application.Common.Interfaces;
using Meetlr.Application.Interfaces;
using Meetlr.Module.Board.Domain.Entities;
using Meetlr.Domain.Exceptions.DomainErrors.ExceptionByArea;
using Microsoft.EntityFrameworkCore;

namespace Meetlr.Module.Board.Application.Commands.ReorderColumns;

public class ReorderColumnsCommandHandler : IRequestHandler<ReorderColumnsCommand, ReorderColumnsCommandResponse>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly ICurrentUserService _currentUserService;

    public ReorderColumnsCommandHandler(
        IUnitOfWork unitOfWork,
        ICurrentUserService currentUserService)
    {
        _unitOfWork = unitOfWork;
        _currentUserService = currentUserService;
    }

    public async Task<ReorderColumnsCommandResponse> Handle(ReorderColumnsCommand request, CancellationToken cancellationToken)
    {
        var userId = _currentUserService.UserId ?? throw AuthenticationErrors.UserNotAuthenticated();

        // Verify board exists and belongs to user
        var board = await _unitOfWork.Repository<Domain.Entities.Board>().GetQueryable()
            .FirstOrDefaultAsync(b => b.Id == request.BoardId && b.UserId == userId && !b.IsDeleted, cancellationToken);

        if (board == null)
        {
            throw BoardErrors.BoardNotFound(request.BoardId);
        }

        var columns = await _unitOfWork.Repository<BoardColumn>().GetQueryable()
            .Where(c => c.BoardId == request.BoardId && !c.IsDeleted)
            .ToListAsync(cancellationToken);

        var columnIdsList = request.ColumnIds.ToList();

        for (int i = 0; i < columnIdsList.Count; i++)
        {
            var column = columns.FirstOrDefault(c => c.Id == columnIdsList[i]);
            if (column != null)
            {
                column.Position = i;
                _unitOfWork.Repository<BoardColumn>().Update(column);
            }
        }

        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return new ReorderColumnsCommandResponse
        {
            Success = true
        };
    }
}
