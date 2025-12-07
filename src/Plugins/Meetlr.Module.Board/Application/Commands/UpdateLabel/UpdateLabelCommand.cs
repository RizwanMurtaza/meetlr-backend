using MediatR;

namespace Meetlr.Module.Board.Application.Commands.UpdateLabel;

public record UpdateLabelCommand : IRequest<UpdateLabelCommandResponse>
{
    public Guid BoardId { get; init; }
    public Guid Id { get; init; }
    public string? Name { get; init; }
    public string? Color { get; init; }
}
