namespace Meetlr.Module.Board.Endpoints.Boards.GetAllBoards;

public record BoardSummaryResponse
{
    public Guid Id { get; init; }
    public string Name { get; init; } = string.Empty;
    public string? Description { get; init; }
    public string Color { get; init; } = string.Empty;
    public int Position { get; init; }
    public DateTime CreatedAt { get; init; }
    public DateTime? ModifiedAt { get; init; }
}

public record GetAllBoardsResponse
{
    public IEnumerable<BoardSummaryResponse> Boards { get; init; } = Array.Empty<BoardSummaryResponse>();
    public int TotalCount { get; init; }
    public int PageNumber { get; init; }
    public int PageSize { get; init; }
    public int TotalPages { get; init; }
    public bool HasPreviousPage { get; init; }
    public bool HasNextPage { get; init; }
}
