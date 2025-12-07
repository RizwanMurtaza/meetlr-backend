namespace Meetlr.Module.Board.Endpoints.Boards.CreateBoard;

public record CreateBoardResponse
{
    public Guid Id { get; init; }
    public string Name { get; init; } = string.Empty;
    public string? Description { get; init; }
    public string Color { get; init; } = string.Empty;
    public int Position { get; init; }
    public DateTime CreatedAt { get; init; }
}
