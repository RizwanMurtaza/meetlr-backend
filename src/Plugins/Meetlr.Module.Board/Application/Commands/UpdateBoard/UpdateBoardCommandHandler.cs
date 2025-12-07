using MediatR;
using Meetlr.Application.Common.Interfaces;
using Meetlr.Application.Interfaces;
using Meetlr.Domain.Exceptions.DomainErrors.ExceptionByArea;
using Microsoft.EntityFrameworkCore;

namespace Meetlr.Module.Board.Application.Commands.UpdateBoard;

public class UpdateBoardCommandHandler : IRequestHandler<UpdateBoardCommand, UpdateBoardCommandResponse>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly ICurrentUserService _currentUserService;

    public UpdateBoardCommandHandler(
        IUnitOfWork unitOfWork,
        ICurrentUserService currentUserService)
    {
        _unitOfWork = unitOfWork;
        _currentUserService = currentUserService;
    }

    public async Task<UpdateBoardCommandResponse> Handle(UpdateBoardCommand request, CancellationToken cancellationToken)
    {
        var userId = _currentUserService.UserId ?? throw AuthenticationErrors.UserNotAuthenticated();

        var board = await _unitOfWork.Repository<Domain.Entities.Board>().GetQueryable()
            .FirstOrDefaultAsync(b => b.Id == request.Id && b.UserId == userId && !b.IsDeleted, cancellationToken);

        if (board == null)
        {
            throw BoardErrors.BoardNotFound(request.Id);
        }

        if (!string.IsNullOrWhiteSpace(request.Name))
            board.Name = request.Name;

        if (request.Description != null)
            board.Description = request.Description;

        if (!string.IsNullOrWhiteSpace(request.Color))
            board.Color = request.Color;

        _unitOfWork.Repository<Domain.Entities.Board>().Update(board);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return new UpdateBoardCommandResponse
        {
            Id = board.Id,
            Name = board.Name,
            Description = board.Description,
            Color = board.Color ?? string.Empty,
            Position = board.Position,
            CreatedAt = board.CreatedAt,
            ModifiedAt = board.UpdatedAt
        };
    }
}
