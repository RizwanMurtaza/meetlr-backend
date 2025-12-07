using MediatR;

namespace Meetlr.Module.Board.Application.Commands.UpdateColumn;

public record UpdateColumnCommand : IRequest<UpdateColumnCommandResponse>
{
    public Guid BoardId { get; init; }
    public Guid Id { get; init; }
    public string Name { get; init; } = string.Empty;
}
