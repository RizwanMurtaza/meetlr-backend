using MediatR;

namespace Meetlr.Module.Board.Application.Commands.DeleteColumn;

public record DeleteColumnCommand : IRequest<DeleteColumnCommandResponse>
{
    public Guid BoardId { get; init; }
    public Guid Id { get; init; }
}
