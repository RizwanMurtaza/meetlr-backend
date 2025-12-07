namespace Meetlr.Module.Board.Application.Queries.GetAllBoards;

public record GetAllBoardsQueryResponse
{
    public IEnumerable<BoardDto> Boards { get; init; } = Array.Empty<BoardDto>();
    public int TotalCount { get; init; }
    public int PageNumber { get; init; }
    public int PageSize { get; init; }
    public int TotalPages { get; init; }
    public bool HasPreviousPage { get; init; }
    public bool HasNextPage { get; init; }
}
