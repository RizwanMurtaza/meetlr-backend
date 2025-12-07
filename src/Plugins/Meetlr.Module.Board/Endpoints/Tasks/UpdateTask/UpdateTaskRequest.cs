using Meetlr.Domain.Enums;

namespace Meetlr.Module.Board.Endpoints.Tasks.UpdateTask;

public record UpdateTaskRequest
{
    public string? Title { get; init; }
    public string? Description { get; init; }
    public DateTime? DueDate { get; init; }
    public TaskPriority? Priority { get; init; }
    public IEnumerable<Guid>? LabelIds { get; init; }
}
