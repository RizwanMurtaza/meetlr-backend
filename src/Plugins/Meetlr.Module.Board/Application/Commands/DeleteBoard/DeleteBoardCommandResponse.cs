namespace Meetlr.Module.Board.Application.Commands.DeleteBoard;

public record DeleteBoardCommandResponse
{
    public bool Success { get; init; }
    public Guid Id { get; init; }
}
