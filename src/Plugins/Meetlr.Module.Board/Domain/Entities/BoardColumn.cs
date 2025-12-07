using Meetlr.Domain.Common;

namespace Meetlr.Module.Board.Domain.Entities;

/// <summary>
/// Represents a column/status in a Kanban board
/// </summary>
public class BoardColumn : BaseAuditableEntity
{
    /// <summary>
    /// Parent board ID
    /// </summary>
    public Guid BoardId { get; set; }

    /// <summary>
    /// Column name (e.g., "To Do", "In Progress", "Done")
    /// </summary>
    public string Name { get; set; } = string.Empty;

    /// <summary>
    /// Order position within the board
    /// </summary>
    public int Position { get; set; }

    // Navigation properties
    public Board Board { get; set; } = null!;
    public ICollection<BoardTask> Tasks { get; set; } = new List<BoardTask>();
}
