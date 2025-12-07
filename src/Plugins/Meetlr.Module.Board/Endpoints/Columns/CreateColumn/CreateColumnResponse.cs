namespace Meetlr.Module.Board.Endpoints.Columns.CreateColumn;

public record CreateColumnResponse
{
    public Guid Id { get; init; }
    public string Name { get; init; } = string.Empty;
    public int Position { get; init; }
}
