using MediatR;

namespace Meetlr.Module.Board.Application.Queries.GetAllBoards;

public record GetAllBoardsQuery : IRequest<GetAllBoardsQueryResponse>
{
    public int PageNumber { get; init; } = 1;
    public int PageSize { get; init; } = 20;
    public string? SearchTerm { get; init; }
    public string? SortBy { get; init; } = "Position";
    public bool SortDescending { get; init; } = false;
}
