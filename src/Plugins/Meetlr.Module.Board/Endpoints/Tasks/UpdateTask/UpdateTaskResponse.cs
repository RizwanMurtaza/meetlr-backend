using Meetlr.Domain.Enums;

namespace Meetlr.Module.Board.Endpoints.Tasks.UpdateTask;

public record UpdateTaskResponse
{
    public Guid Id { get; init; }
    public string Title { get; init; } = string.Empty;
    public string? Description { get; init; }
    public DateTime? DueDate { get; init; }
    public TaskPriority Priority { get; init; }
    public IEnumerable<Guid> LabelIds { get; init; } = Array.Empty<Guid>();
}
