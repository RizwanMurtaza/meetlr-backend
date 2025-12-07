using Meetlr.Domain.Enums;

namespace Meetlr.Module.Board.Endpoints.Tasks.CreateTask;

public record CreateTaskRequest
{
    public Guid ColumnId { get; init; }
    public string Title { get; init; } = string.Empty;
    public string? Description { get; init; }
    public DateTime? DueDate { get; init; }
    public TaskPriority Priority { get; init; } = TaskPriority.Medium;
    public IEnumerable<Guid> LabelIds { get; init; } = Array.Empty<Guid>();
}
