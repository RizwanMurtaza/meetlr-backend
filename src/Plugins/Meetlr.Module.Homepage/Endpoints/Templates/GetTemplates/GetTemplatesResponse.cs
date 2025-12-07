namespace Meetlr.Module.Homepage.Endpoints.Templates.GetTemplates;

public record GetTemplatesResponse
{
    public IEnumerable<TemplateResponse> Templates { get; init; } = Array.Empty<TemplateResponse>();
}

public record TemplateResponse
{
    public Guid Id { get; init; }
    public string Name { get; init; } = string.Empty;
    public string Slug { get; init; } = string.Empty;
    public string Description { get; init; } = string.Empty;
    public string Category { get; init; } = string.Empty;
    public string PreviewImageUrl { get; init; } = string.Empty;
    public string? PreviewImageUrlMobile { get; init; }
    public string ComponentName { get; init; } = string.Empty;
    public string DefaultPrimaryColor { get; init; } = string.Empty;
    public string DefaultSecondaryColor { get; init; } = string.Empty;
    public string DefaultAccentColor { get; init; } = string.Empty;
    public string DefaultBackgroundColor { get; init; } = string.Empty;
    public string DefaultTextColor { get; init; } = string.Empty;
    public string DefaultFontFamily { get; init; } = string.Empty;
    public List<TemplateSectionResponse> AvailableSections { get; init; } = new();
    public List<string> DefaultEnabledSections { get; init; } = new();
    public bool IsFree { get; init; }
    public int DisplayOrder { get; init; }
}

public record TemplateSectionResponse
{
    public string Id { get; init; } = string.Empty;
    public string Name { get; init; } = string.Empty;
    public bool Required { get; init; }
    public string? Description { get; init; }
    public string? Icon { get; init; }
}
