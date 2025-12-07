namespace Meetlr.Module.Board.Domain.Entities;

/// <summary>
/// Join table for many-to-many relationship between BoardTask and BoardLabel
/// </summary>
public class BoardTaskLabel
{
    public Guid TaskId { get; set; }
    public Guid LabelId { get; set; }

    // Navigation properties
    public BoardTask Task { get; set; } = null!;
    public BoardLabel Label { get; set; } = null!;
}
