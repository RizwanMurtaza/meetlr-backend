using MediatR;

namespace Meetlr.Module.Board.Application.Commands.CreateColumn;

public record CreateColumnCommand : IRequest<CreateColumnCommandResponse>
{
    public Guid BoardId { get; init; }
    public string Name { get; init; } = string.Empty;
}
