using Meetlr.Domain.Common;

namespace Meetlr.Module.Board.Domain.Entities;

/// <summary>
/// Represents a label that can be applied to tasks within a board
/// </summary>
public class BoardLabel : BaseAuditableEntity
{
    /// <summary>
    /// Parent board ID
    /// </summary>
    public Guid BoardId { get; set; }

    /// <summary>
    /// Label name
    /// </summary>
    public string Name { get; set; } = string.Empty;

    /// <summary>
    /// Hex color for the label
    /// </summary>
    public string Color { get; set; } = "#6366f1"; // Default indigo

    // Navigation properties
    public Board Board { get; set; } = null!;
    public ICollection<BoardTask> Tasks { get; set; } = new List<BoardTask>();
}
