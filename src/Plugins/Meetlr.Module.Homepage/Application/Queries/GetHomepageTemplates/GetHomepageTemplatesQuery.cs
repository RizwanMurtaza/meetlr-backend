using MediatR;

namespace Meetlr.Module.Homepage.Application.Queries.GetHomepageTemplates;

public record GetHomepageTemplatesQuery : IRequest<GetHomepageTemplatesResponse>
{
    public string? Category { get; init; }
    public bool? IsFree { get; init; }
    public bool ActiveOnly { get; init; } = true;
}
