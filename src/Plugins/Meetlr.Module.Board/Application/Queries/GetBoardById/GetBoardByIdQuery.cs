using MediatR;

namespace Meetlr.Module.Board.Application.Queries.GetBoardById;

public record GetBoardByIdQuery : IRequest<GetBoardByIdQueryResponse>
{
    public Guid Id { get; init; }
}
