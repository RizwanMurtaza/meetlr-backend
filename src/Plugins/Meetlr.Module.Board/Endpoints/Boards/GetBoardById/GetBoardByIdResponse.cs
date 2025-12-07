using Meetlr.Domain.Enums;

namespace Meetlr.Module.Board.Endpoints.Boards.GetBoardById;

public record GetBoardByIdResponse
{
    public Guid Id { get; init; }
    public string Name { get; init; } = string.Empty;
    public string? Description { get; init; }
    public string Color { get; init; } = string.Empty;
    public int Position { get; init; }
    public DateTime CreatedAt { get; init; }
    public IEnumerable<ColumnDetailResponse> Columns { get; init; } = Array.Empty<ColumnDetailResponse>();
    public IEnumerable<LabelDetailResponse> Labels { get; init; } = Array.Empty<LabelDetailResponse>();
}

public record ColumnDetailResponse
{
    public Guid Id { get; init; }
    public string Name { get; init; } = string.Empty;
    public int Position { get; init; }
    public IEnumerable<TaskDetailResponse> Tasks { get; init; } = Array.Empty<TaskDetailResponse>();
}

public record TaskDetailResponse
{
    public Guid Id { get; init; }
    public string Title { get; init; } = string.Empty;
    public string? Description { get; init; }
    public DateTime? DueDate { get; init; }
    public TaskPriority Priority { get; init; }
    public int Position { get; init; }
    public IEnumerable<Guid> LabelIds { get; init; } = Array.Empty<Guid>();
}

public record LabelDetailResponse
{
    public Guid Id { get; init; }
    public string Name { get; init; } = string.Empty;
    public string Color { get; init; } = string.Empty;
}
