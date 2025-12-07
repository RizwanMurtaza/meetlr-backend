namespace Meetlr.Module.Board.Endpoints.Labels.UpdateLabel;

public record UpdateLabelRequest
{
    public string? Name { get; init; }
    public string? Color { get; init; }
}
