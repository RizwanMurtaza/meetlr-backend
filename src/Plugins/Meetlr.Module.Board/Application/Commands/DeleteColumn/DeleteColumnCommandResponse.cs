namespace Meetlr.Module.Board.Application.Commands.DeleteColumn;

public record DeleteColumnCommandResponse
{
    public bool Success { get; init; }
    public Guid Id { get; init; }
}
