namespace Meetlr.Module.Homepage.Application.Queries.GetHomepageTemplates;

public record GetHomepageTemplatesResponse
{
    public List<HomepageTemplateDto> Templates { get; init; } = new();
    public int TotalCount { get; init; }
}
