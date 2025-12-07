namespace Meetlr.Module.Board.Application.Commands.UpdateLabel;

public record UpdateLabelCommandResponse
{
    public Guid Id { get; init; }
    public string Name { get; init; } = string.Empty;
    public string Color { get; init; } = string.Empty;
}
