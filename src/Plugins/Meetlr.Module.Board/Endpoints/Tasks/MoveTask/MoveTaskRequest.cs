namespace Meetlr.Module.Board.Endpoints.Tasks.MoveTask;

public record MoveTaskRequest
{
    public Guid TargetColumnId { get; init; }
    public int NewPosition { get; init; }
}
