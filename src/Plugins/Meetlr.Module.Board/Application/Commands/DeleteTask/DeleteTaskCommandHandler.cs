using MediatR;
using Meetlr.Application.Common.Interfaces;
using Meetlr.Application.Interfaces;
using Meetlr.Module.Board.Domain.Entities;
using Meetlr.Domain.Exceptions.DomainErrors.ExceptionByArea;
using Microsoft.EntityFrameworkCore;

namespace Meetlr.Module.Board.Application.Commands.DeleteTask;

public class DeleteTaskCommandHandler : IRequestHandler<DeleteTaskCommand, DeleteTaskCommandResponse>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly ICurrentUserService _currentUserService;

    public DeleteTaskCommandHandler(
        IUnitOfWork unitOfWork,
        ICurrentUserService currentUserService)
    {
        _unitOfWork = unitOfWork;
        _currentUserService = currentUserService;
    }

    public async Task<DeleteTaskCommandResponse> Handle(DeleteTaskCommand request, CancellationToken cancellationToken)
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
            .FirstOrDefaultAsync(t => t.Id == request.Id && t.Column.BoardId == request.BoardId && !t.IsDeleted, cancellationToken);

        if (task == null)
        {
            throw BoardErrors.TaskNotFound(request.Id);
        }

        task.IsDeleted = true;
        _unitOfWork.Repository<BoardTask>().Update(task);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return new DeleteTaskCommandResponse
        {
            Success = true
        };
    }
}
