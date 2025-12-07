namespace Meetlr.Module.Board.Application.Commands.CreateColumn;

public record CreateColumnCommandResponse
{
    public Guid Id { get; init; }
    public string Name { get; init; } = string.Empty;
    public int Position { get; init; }
}
