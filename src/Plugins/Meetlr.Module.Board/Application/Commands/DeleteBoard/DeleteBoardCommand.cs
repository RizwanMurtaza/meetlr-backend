using MediatR;

namespace Meetlr.Module.Board.Application.Commands.DeleteBoard;

public record DeleteBoardCommand : IRequest<DeleteBoardCommandResponse>
{
    public Guid Id { get; init; }
}
