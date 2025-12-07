namespace Meetlr.Module.Board.Application.Commands.UpdateColumn;

public record UpdateColumnCommandResponse
{
    public Guid Id { get; init; }
    public string Name { get; init; } = string.Empty;
    public int Position { get; init; }
}
