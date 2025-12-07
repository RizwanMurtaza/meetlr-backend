using MediatR;
using Meetlr.Application.Common.Interfaces;
using Meetlr.Application.Interfaces;
using Meetlr.Module.Board.Domain.Entities;
using Meetlr.Domain.Exceptions.DomainErrors.ExceptionByArea;
using Microsoft.EntityFrameworkCore;

namespace Meetlr.Module.Board.Application.Commands.CreateTask;

public class CreateTaskCommandHandler : IRequestHandler<CreateTaskCommand, CreateTaskCommandResponse>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly ICurrentUserService _currentUserService;

    public CreateTaskCommandHandler(
        IUnitOfWork unitOfWork,
        ICurrentUserService currentUserService)
    {
        _unitOfWork = unitOfWork;
        _currentUserService = currentUserService;
    }

    public async Task<CreateTaskCommandResponse> Handle(CreateTaskCommand request, CancellationToken cancellationToken)
    {
        var userId = _currentUserService.UserId ?? throw AuthenticationErrors.UserNotAuthenticated();

        // Verify board exists and belongs to user
        var board = await _unitOfWork.Repository<Domain.Entities.Board>().GetQueryable()
            .FirstOrDefaultAsync(b => b.Id == request.BoardId && b.UserId == userId && !b.IsDeleted, cancellationToken);

        if (board == null)
        {
            throw BoardErrors.BoardNotFound(request.BoardId);
        }

        // Verify column exists and belongs to the board
        var column = await _unitOfWork.Repository<BoardColumn>().GetQueryable()
            .FirstOrDefaultAsync(c => c.Id == request.ColumnId && c.BoardId == request.BoardId && !c.IsDeleted, cancellationToken);

        if (column == null)
        {
            throw BoardErrors.ColumnNotFound(request.ColumnId);
        }

        // Get max position for tasks in this column
        var maxPosition = await _unitOfWork.Repository<BoardTask>().GetQueryable()
            .Where(t => t.ColumnId == request.ColumnId && !t.IsDeleted)
            .MaxAsync(t => (int?)t.Position, cancellationToken) ?? -1;

        var task = new BoardTask
        {
            ColumnId = request.ColumnId,
            Title = request.Title,
            Description = request.Description,
            DueDate = request.DueDate,
            Priority = request.Priority,
            Position = maxPosition + 1
        };

        // Add labels if provided
        if (request.LabelIds.Any())
        {
            var labels = await _unitOfWork.Repository<BoardLabel>().GetQueryable()
                .Where(l => request.LabelIds.Contains(l.Id) && l.BoardId == request.BoardId && !l.IsDeleted)
                .ToListAsync(cancellationToken);

            foreach (var label in labels)
            {
                task.Labels.Add(label);
            }
        }

        _unitOfWork.Repository<BoardTask>().Add(task);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return new CreateTaskCommandResponse
        {
            Id = task.Id,
            ColumnId = task.ColumnId,
            Title = task.Title,
            Description = task.Description,
            DueDate = task.DueDate,
            Position = task.Position,
            Priority = task.Priority,
            LabelIds = task.Labels.Select(l => l.Id).ToList(),
            CreatedAt = task.CreatedAt
        };
    }
}
