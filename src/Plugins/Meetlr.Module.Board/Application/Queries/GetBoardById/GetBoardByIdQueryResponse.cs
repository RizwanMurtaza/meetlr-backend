namespace Meetlr.Module.Board.Application.Queries.GetBoardById;

public record GetBoardByIdQueryResponse
{
    public BoardDetailDto? Board { get; init; }
}
