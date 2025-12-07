using MediatR;
using Meetlr.Application.Common.Interfaces;
using Meetlr.Application.Interfaces;
using Meetlr.Module.Board.Domain.Entities;
using Meetlr.Domain.Exceptions.DomainErrors.ExceptionByArea;
using Microsoft.EntityFrameworkCore;

namespace Meetlr.Module.Board.Application.Commands.CreateColumn;

public class CreateColumnCommandHandler : IRequestHandler<CreateColumnCommand, CreateColumnCommandResponse>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly ICurrentUserService _currentUserService;

    public CreateColumnCommandHandler(
        IUnitOfWork unitOfWork,
        ICurrentUserService currentUserService)
    {
        _unitOfWork = unitOfWork;
        _currentUserService = currentUserService;
    }

    public async Task<CreateColumnCommandResponse> Handle(CreateColumnCommand request, CancellationToken cancellationToken)
    {
        var userId = _currentUserService.UserId ?? throw AuthenticationErrors.UserNotAuthenticated();

        // Verify board exists and belongs to user
        var board = await _unitOfWork.Repository<Domain.Entities.Board>().GetQueryable()
            .FirstOrDefaultAsync(b => b.Id == request.BoardId && b.UserId == userId && !b.IsDeleted, cancellationToken);

        if (board == null)
        {
            throw BoardErrors.BoardNotFound(request.BoardId);
        }

        // Get max position for columns in this board
        var maxPosition = await _unitOfWork.Repository<BoardColumn>().GetQueryable()
            .Where(c => c.BoardId == request.BoardId && !c.IsDeleted)
            .MaxAsync(c => (int?)c.Position, cancellationToken) ?? -1;

        var column = new BoardColumn
        {
            BoardId = request.BoardId,
            Name = request.Name,
            Position = maxPosition + 1
        };

        _unitOfWork.Repository<BoardColumn>().Add(column);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return new CreateColumnCommandResponse
        {
            Id = column.Id,
            Name = column.Name,
            Position = column.Position
        };
    }
}
