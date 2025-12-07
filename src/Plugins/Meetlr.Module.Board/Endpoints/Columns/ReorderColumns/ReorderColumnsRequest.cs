namespace Meetlr.Module.Board.Endpoints.Columns.ReorderColumns;

public record ReorderColumnsRequest
{
    public IEnumerable<Guid> ColumnIds { get; init; } = Array.Empty<Guid>();
}
