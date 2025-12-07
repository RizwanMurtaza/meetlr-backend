namespace Meetlr.Module.Homepage.Endpoints.Templates.GetTemplates;

public record GetTemplatesRequest
{
    public string? Category { get; init; }
    public bool? IsFree { get; init; }
    public bool ActiveOnly { get; init; } = true;
}
