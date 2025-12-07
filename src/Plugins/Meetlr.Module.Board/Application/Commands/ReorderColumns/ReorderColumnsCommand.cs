using MediatR;

namespace Meetlr.Module.Board.Application.Commands.ReorderColumns;

public record ReorderColumnsCommand : IRequest<ReorderColumnsCommandResponse>
{
    public Guid BoardId { get; init; }
    public IEnumerable<Guid> ColumnIds { get; init; } = Array.Empty<Guid>();
}
