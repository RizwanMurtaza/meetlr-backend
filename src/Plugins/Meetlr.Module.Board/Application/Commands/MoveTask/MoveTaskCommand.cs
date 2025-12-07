using MediatR;

namespace Meetlr.Module.Board.Application.Commands.MoveTask;

public record MoveTaskCommand : IRequest<MoveTaskCommandResponse>
{
    public Guid BoardId { get; init; }
    public Guid TaskId { get; init; }
    public Guid TargetColumnId { get; init; }
    public int NewPosition { get; init; }
}
