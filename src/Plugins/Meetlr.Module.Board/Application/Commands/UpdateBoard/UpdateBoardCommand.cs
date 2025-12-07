using MediatR;

namespace Meetlr.Module.Board.Application.Commands.UpdateBoard;

public record UpdateBoardCommand : IRequest<UpdateBoardCommandResponse>
{
    public Guid Id { get; init; }
    public string? Name { get; init; }
    public string? Description { get; init; }
    public string? Color { get; init; }
}
