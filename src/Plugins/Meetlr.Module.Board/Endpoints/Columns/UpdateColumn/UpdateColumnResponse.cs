namespace Meetlr.Module.Board.Endpoints.Columns.UpdateColumn;

public record UpdateColumnResponse
{
    public Guid Id { get; init; }
    public string Name { get; init; } = string.Empty;
    public int Position { get; init; }
}
