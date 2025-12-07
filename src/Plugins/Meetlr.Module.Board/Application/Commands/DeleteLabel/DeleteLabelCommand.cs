using MediatR;

namespace Meetlr.Module.Board.Application.Commands.DeleteLabel;

public record DeleteLabelCommand : IRequest<DeleteLabelCommandResponse>
{
    public Guid BoardId { get; init; }
    public Guid Id { get; init; }
}
