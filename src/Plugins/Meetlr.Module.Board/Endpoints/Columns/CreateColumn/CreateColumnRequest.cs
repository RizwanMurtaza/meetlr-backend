namespace Meetlr.Module.Board.Endpoints.Columns.CreateColumn;

public record CreateColumnRequest
{
    public string Name { get; init; } = string.Empty;
}
