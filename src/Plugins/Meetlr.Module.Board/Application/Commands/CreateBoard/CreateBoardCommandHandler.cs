using MediatR;
using Meetlr.Application.Common.Interfaces;
using Meetlr.Application.Interfaces;
using Meetlr.Domain.Exceptions.DomainErrors.ExceptionByArea;
using Meetlr.Module.Board.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace Meetlr.Module.Board.Application.Commands.CreateBoard;

public class CreateBoardCommandHandler : IRequestHandler<CreateBoardCommand, CreateBoardCommandResponse>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly ICurrentUserService _currentUserService;

    public CreateBoardCommandHandler(
        IUnitOfWork unitOfWork,
        ICurrentUserService currentUserService)
    {
        _unitOfWork = unitOfWork;
        _currentUserService = currentUserService;
    }

    public async Task<CreateBoardCommandResponse> Handle(CreateBoardCommand request, CancellationToken cancellationToken)
    {
        var userId = _currentUserService.UserId ?? throw AuthenticationErrors.UserNotAuthenticated();

        // Get the max position for the user's boards
        var maxPosition = await _unitOfWork.Repository<Domain.Entities.Board>().GetQueryable()
            .Where(b => b.UserId == userId && !b.IsDeleted)
            .MaxAsync(b => (int?)b.Position, cancellationToken) ?? -1;

        var board = new Domain.Entities.Board
        {
            UserId = userId,
            Name = request.Name,
            Description = request.Description,
            Color = request.Color ?? "#6366f1", // Default indigo
            Position = maxPosition + 1
        };

        // Create default columns
        var defaultColumns = new[]
        {
            new BoardColumn { Name = "To Do", Position = 0 },
            new BoardColumn { Name = "In Progress", Position = 1 },
            new BoardColumn { Name = "Done", Position = 2 }
        };

        foreach (var column in defaultColumns)
        {
            board.Columns.Add(column);
        }

        _unitOfWork.Repository<Domain.Entities.Board>().Add(board);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return new CreateBoardCommandResponse
        {
            Id = board.Id,
            Name = board.Name,
            Description = board.Description,
            Color = board.Color ?? string.Empty,
            Position = board.Position,
            CreatedAt = board.CreatedAt
        };
    }
}
