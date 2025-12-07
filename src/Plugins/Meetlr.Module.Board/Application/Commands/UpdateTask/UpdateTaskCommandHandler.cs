using MediatR;
using Meetlr.Application.Common.Interfaces;
using Meetlr.Application.Interfaces;
using Meetlr.Module.Board.Domain.Entities;
using Meetlr.Domain.Exceptions.DomainErrors.ExceptionByArea;
using Microsoft.EntityFrameworkCore;

namespace Meetlr.Module.Board.Application.Commands.UpdateTask;

public class UpdateTaskCommandHandler : IRequestHandler<UpdateTaskCommand, UpdateTaskCommandResponse>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly ICurrentUserService _currentUserService;

    public UpdateTaskCommandHandler(
        IUnitOfWork unitOfWork,
        ICurrentUserService currentUserService)
    {
        _unitOfWork = unitOfWork;
        _currentUserService = currentUserService;
    }

    public async Task<UpdateTaskCommandResponse> Handle(UpdateTaskCommand request, CancellationToken cancellationToken)
    {
        var userId = _currentUserService.UserId ?? throw AuthenticationErrors.UserNotAuthenticated();

        // Verify board exists and belongs to user
        var board = await _unitOfWork.Repository<Domain.Entities.Board>().GetQueryable()
            .FirstOrDefaultAsync(b => b.Id == request.BoardId && b.UserId == userId && !b.IsDeleted, cancellationToken);

        if (board == null)
        {
            throw BoardErrors.BoardNotFound(request.BoardId);
        }

        var task = await _unitOfWork.Repository<BoardTask>().GetQueryable()
            .Include(t => t.Column)
            .Include(t => t.Labels)
            .FirstOrDefaultAsync(t => t.Id == request.Id && t.Column.BoardId == request.BoardId && !t.IsDeleted, cancellationToken);

        if (task == null)
        {
            throw BoardErrors.TaskNotFound(request.Id);
        }

        if (!string.IsNullOrWhiteSpace(request.Title))
            task.Title = request.Title;

        if (request.Description != null)
            task.Description = request.Description;

        if (request.DueDate.HasValue)
            task.DueDate = request.DueDate;

        if (request.Priority.HasValue)
            task.Priority = request.Priority.Value;

        // Update labels if provided
        if (request.LabelIds != null)
        {
            task.Labels.Clear();

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
        }

        _unitOfWork.Repository<BoardTask>().Update(task);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return new UpdateTaskCommandResponse
        {
            Id = task.Id,
            Title = task.Title,
            Description = task.Description,
            DueDate = task.DueDate,
            Priority = task.Priority,
            LabelIds = task.Labels.Select(l => l.Id)
        };
    }
}
