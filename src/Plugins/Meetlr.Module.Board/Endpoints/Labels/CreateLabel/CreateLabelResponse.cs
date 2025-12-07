namespace Meetlr.Module.Board.Endpoints.Labels.CreateLabel;

public record CreateLabelResponse
{
    public Guid Id { get; init; }
    public string Name { get; init; } = string.Empty;
    public string Color { get; init; } = string.Empty;
}
