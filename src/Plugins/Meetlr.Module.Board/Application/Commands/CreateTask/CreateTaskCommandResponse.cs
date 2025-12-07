using Meetlr.Domain.Enums;

namespace Meetlr.Module.Board.Application.Commands.CreateTask;

public record CreateTaskCommandResponse
{
    public Guid Id { get; init; }
    public Guid ColumnId { get; init; }
    public string Title { get; init; } = string.Empty;
    public string? Description { get; init; }
    public DateTime? DueDate { get; init; }
    public int Position { get; init; }
    public TaskPriority Priority { get; init; }
    public List<Guid> LabelIds { get; init; } = new();
    public DateTime CreatedAt { get; init; }
}
