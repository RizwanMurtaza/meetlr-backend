using Meetlr.Domain.Common;
using Meetlr.Domain.Entities.Users;

namespace Meetlr.Module.Board.Domain.Entities;

/// <summary>
/// Represents a Kanban board for task management
/// </summary>
public class Board : BaseAuditableEntity
{
    /// <summary>
    /// User who owns this board
    /// </summary>
    public Guid UserId { get; set; }

    /// <summary>
    /// Board name/title
    /// </summary>
    public string Name { get; set; } = string.Empty;

    /// <summary>
    /// Optional board description
    /// </summary>
    public string? Description { get; set; }

    /// <summary>
    /// Hex color for the board card display
    /// </summary>
    public string? Color { get; set; }

    /// <summary>
    /// Order position in the user's board list
    /// </summary>
    public int Position { get; set; }

    // Navigation properties
    public User Owner { get; set; } = null!;
    public ICollection<BoardColumn> Columns { get; set; } = new List<BoardColumn>();
    public ICollection<BoardLabel> Labels { get; set; } = new List<BoardLabel>();
}
