using MediatR;

namespace Meetlr.Module.Board.Application.Commands.DeleteTask;

public record DeleteTaskCommand : IRequest<DeleteTaskCommandResponse>
{
    public Guid BoardId { get; init; }
    public Guid Id { get; init; }
}
