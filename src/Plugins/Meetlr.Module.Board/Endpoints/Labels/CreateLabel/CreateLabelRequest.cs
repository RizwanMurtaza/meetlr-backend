namespace Meetlr.Module.Board.Endpoints.Labels.CreateLabel;

public record CreateLabelRequest
{
    public string Name { get; init; } = string.Empty;
    public string Color { get; init; } = "#6366f1";
}
