using Meetlr.Domain.Common;
using Meetlr.Domain.Enums;

namespace Meetlr.Module.Board.Domain.Entities;

/// <summary>
/// Represents a task/card in a Kanban board column
/// </summary>
public class BoardTask : BaseAuditableEntity
{
    /// <summary>
    /// Parent column ID
    /// </summary>
    public Guid ColumnId { get; set; }

    /// <summary>
    /// Task title
    /// </summary>
    public string Title { get; set; } = string.Empty;

    /// <summary>
    /// Optional task description
    /// </summary>
    public string? Description { get; set; }

    /// <summary>
    /// Optional due date
    /// </summary>
    public DateTime? DueDate { get; set; }

    /// <summary>
    /// Order position within the column
    /// </summary>
    public int Position { get; set; }

    /// <summary>
    /// Task priority level
    /// </summary>
    public TaskPriority Priority { get; set; } = TaskPriority.Medium;

    // Navigation properties
    public BoardColumn Column { get; set; } = null!;
    public ICollection<BoardLabel> Labels { get; set; } = new List<BoardLabel>();
}
