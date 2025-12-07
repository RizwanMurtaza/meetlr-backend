namespace Meetlr.Module.Board.Endpoints.Boards.GetAllBoards;

public record GetAllBoardsRequest
{
    public int PageNumber { get; init; } = 1;
    public int PageSize { get; init; } = 10;
}
