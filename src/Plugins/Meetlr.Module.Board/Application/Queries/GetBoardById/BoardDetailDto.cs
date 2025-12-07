using Meetlr.Domain.Enums;

namespace Meetlr.Module.Board.Application.Queries.GetBoardById;

public record BoardDetailDto
{
    public Guid Id { get; init; }
    public string Name { get; init; } = string.Empty;
    public string? Description { get; init; }
    public string? Color { get; init; }
    public int Position { get; init; }
    public DateTime CreatedAt { get; init; }
    public DateTime? UpdatedAt { get; init; }
    public IEnumerable<ColumnDto> Columns { get; init; } = Array.Empty<ColumnDto>();
    public IEnumerable<LabelDto> Labels { get; init; } = Array.Empty<LabelDto>();
}

public record ColumnDto
{
    public Guid Id { get; init; }
    public string Name { get; init; } = string.Empty;
    public int Position { get; init; }
    public IEnumerable<TaskDto> Tasks { get; init; } = Array.Empty<TaskDto>();
}

public record TaskDto
{
    public Guid Id { get; init; }
    public Guid ColumnId { get; init; }
    public string Title { get; init; } = string.Empty;
    public string? Description { get; init; }
    public DateTime? DueDate { get; init; }
    public int Position { get; init; }
    public TaskPriority Priority { get; init; }
    public DateTime CreatedAt { get; init; }
    public DateTime? UpdatedAt { get; init; }
    public IEnumerable<Guid> LabelIds { get; init; } = Array.Empty<Guid>();
}

public record LabelDto
{
    public Guid Id { get; init; }
    public string Name { get; init; } = string.Empty;
    public string Color { get; init; } = string.Empty;
}
