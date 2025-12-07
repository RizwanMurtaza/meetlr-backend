using MediatR;

namespace Meetlr.Module.Board.Application.Commands.CreateLabel;

public record CreateLabelCommand : IRequest<CreateLabelCommandResponse>
{
    public Guid BoardId { get; init; }
    public string Name { get; init; } = string.Empty;
    public string Color { get; init; } = "#6366f1";
}
