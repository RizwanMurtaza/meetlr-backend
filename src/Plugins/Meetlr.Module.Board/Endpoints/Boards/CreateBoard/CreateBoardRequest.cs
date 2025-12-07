namespace Meetlr.Module.Board.Endpoints.Boards.CreateBoard;

public record CreateBoardRequest
{
    public string Name { get; init; } = string.Empty;
    public string? Description { get; init; }
    public string Color { get; init; } = "#6366f1";
}
