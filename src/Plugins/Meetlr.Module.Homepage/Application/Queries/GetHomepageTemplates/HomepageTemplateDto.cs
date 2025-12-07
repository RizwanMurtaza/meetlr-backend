namespace Meetlr.Module.Homepage.Application.Queries.GetHomepageTemplates;

public record HomepageTemplateDto
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
    public List<TemplateSectionDto> AvailableSections { get; init; } = new();
    public List<string> DefaultEnabledSections { get; init; } = new();
    public bool IsActive { get; init; }
    public bool IsFree { get; init; }
    public int DisplayOrder { get; init; }
}

public record TemplateSectionDto
{
    public string Id { get; init; } = string.Empty;
    public string Name { get; init; } = string.Empty;
    public bool Required { get; init; }
    public string? Description { get; init; }
    public string? Icon { get; init; }
}
