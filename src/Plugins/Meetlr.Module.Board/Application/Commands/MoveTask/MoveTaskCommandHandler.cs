using MediatR;
using Meetlr.Application.Common.Interfaces;
using Meetlr.Application.Interfaces;
using Meetlr.Module.Board.Domain.Entities;
using Meetlr.Domain.Exceptions.DomainErrors.ExceptionByArea;
using Microsoft.EntityFrameworkCore;

namespace Meetlr.Module.Board.Application.Commands.MoveTask;

public class MoveTaskCommandHandler : IRequestHandler<MoveTaskCommand, MoveTaskCommandResponse>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly ICurrentUserService _currentUserService;

    public MoveTaskCommandHandler(
        IUnitOfWork unitOfWork,
        ICurrentUserService currentUserService)
    {
        _unitOfWork = unitOfWork;
        _currentUserService = currentUserService;
    }

    public async Task<MoveTaskCommandResponse> Handle(MoveTaskCommand request, CancellationToken cancellationToken)
    {
        var userId = _currentUserService.UserId ?? throw AuthenticationErrors.UserNotAuthenticated();

        // Verify board exists and belongs to user
        var board = await _unitOfWork.Repository<Domain.Entities.Board>().GetQueryable()
            .FirstOrDefaultAsync(b => b.Id == request.BoardId && b.UserId == userId && !b.IsDeleted, cancellationToken);

        if (board == null)
        {
            throw BoardErrors.BoardNotFound(request.BoardId);
        }

        // Verify target column exists and belongs to this board
        var targetColumn = await _unitOfWork.Repository<BoardColumn>().GetQueryable()
            .FirstOrDefaultAsync(c => c.Id == request.TargetColumnId && c.BoardId == request.BoardId && !c.IsDeleted, cancellationToken);

        if (targetColumn == null)
        {
            throw BoardErrors.ColumnNotFound(request.TargetColumnId);
        }

        var task = await _unitOfWork.Repository<BoardTask>().GetQueryable()
            .Include(t => t.Column)
            .FirstOrDefaultAsync(t => t.Id == request.TaskId && t.Column.BoardId == request.BoardId && !t.IsDeleted, cancellationToken);

        if (task == null)
        {
            throw BoardErrors.TaskNotFound(request.TaskId);
        }

        var sourceColumnId = task.ColumnId;
        var oldPosition = task.Position;
        var isMovingToNewColumn = sourceColumnId != request.TargetColumnId;

        if (isMovingToNewColumn)
        {
            // Reorder tasks in source column (shift down positions above the removed task)
            var sourceColumnTasks = await _unitOfWork.Repository<BoardTask>().GetQueryable()
                .Where(t => t.ColumnId == sourceColumnId && !t.IsDeleted && t.Position > oldPosition)
                .ToListAsync(cancellationToken);

            foreach (var t in sourceColumnTasks)
            {
                t.Position--;
                _unitOfWork.Repository<BoardTask>().Update(t);
            }

            // Reorder tasks in target column (shift up positions at and above the new position)
            var targetColumnTasks = await _unitOfWork.Repository<BoardTask>().GetQueryable()
                .Where(t => t.ColumnId == request.TargetColumnId && !t.IsDeleted && t.Position >= request.NewPosition)
                .ToListAsync(cancellationToken);

            foreach (var t in targetColumnTasks)
            {
                t.Position++;
                _unitOfWork.Repository<BoardTask>().Update(t);
            }

            // Move the task to the new column
            task.ColumnId = request.TargetColumnId;
            task.Position = request.NewPosition;
        }
        else
        {
            // Moving within the same column
            if (request.NewPosition == oldPosition)
            {
                return new MoveTaskCommandResponse { Success = true };
            }

            var tasksInColumn = await _unitOfWork.Repository<BoardTask>().GetQueryable()
                .Where(t => t.ColumnId == sourceColumnId && !t.IsDeleted && t.Id != request.TaskId)
                .ToListAsync(cancellationToken);

            if (request.NewPosition > oldPosition)
            {
                // Moving down: shift tasks between old and new position up
                foreach (var t in tasksInColumn.Where(t => t.Position > oldPosition && t.Position <= request.NewPosition))
                {
                    t.Position--;
                    _unitOfWork.Repository<BoardTask>().Update(t);
                }
            }
            else
            {
                // Moving up: shift tasks between new and old position down
                foreach (var t in tasksInColumn.Where(t => t.Position >= request.NewPosition && t.Position < oldPosition))
                {
                    t.Position++;
                    _unitOfWork.Repository<BoardTask>().Update(t);
                }
            }

            task.Position = request.NewPosition;
        }

        _unitOfWork.Repository<BoardTask>().Update(task);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return new MoveTaskCommandResponse
        {
            Success = true
        };
    }
}
