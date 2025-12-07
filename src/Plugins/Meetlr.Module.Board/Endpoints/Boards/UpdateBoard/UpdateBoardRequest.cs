namespace Meetlr.Module.Board.Endpoints.Boards.UpdateBoard;

public record UpdateBoardRequest
{
    public string? Name { get; init; }
    public string? Description { get; init; }
    public string? Color { get; init; }
}
