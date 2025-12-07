using MediatR;

namespace Meetlr.Module.Board.Application.Commands.CreateBoard;

public record CreateBoardCommand : IRequest<CreateBoardCommandResponse>
{
    public string Name { get; init; } = string.Empty;
    public string? Description { get; init; }
    public string? Color { get; init; }
}
