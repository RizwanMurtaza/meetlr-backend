using MediatR;
using Meetlr.Module.Homepage.Application.Queries.GetHomepageTemplates;
using Meetlr.Module.Homepage.Endpoints.Common;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace Meetlr.Module.Homepage.Endpoints.Templates.GetTemplates;

[Route("api/homepage/templates")]
public class GetTemplatesEndpoint : BaseAuthenticatedEndpoint
{
    private readonly IMediator _mediator;

    public GetTemplatesEndpoint(IMediator mediator)
    {
        _mediator = mediator;
    }

    /// <summary>
    /// Get all available homepage templates
    /// </summary>
    [HttpGet]
    [ProducesResponseType(typeof(GetTemplatesResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> Handle(
        [FromQuery] string? category = null,
        [FromQuery] bool? isFree = null,
        [FromQuery] bool activeOnly = true)
    {
        var query = new GetHomepageTemplatesQuery
        {
            Category = category,
            IsFree = isFree,
            ActiveOnly = activeOnly
        };

        var queryResponse = await _mediator.Send(query);

        var response = new GetTemplatesResponse
        {
            Templates = queryResponse.Templates.Select(t => new TemplateResponse
            {
                Id = t.Id,
                Name = t.Name,
                Slug = t.Slug,
                Description = t.Description,
                Category = t.Category,
                PreviewImageUrl = t.PreviewImageUrl,
                PreviewImageUrlMobile = t.PreviewImageUrlMobile,
                ComponentName = t.ComponentName,
                DefaultPrimaryColor = t.DefaultPrimaryColor,
                DefaultSecondaryColor = t.DefaultSecondaryColor,
                DefaultAccentColor = t.DefaultAccentColor,
                DefaultBackgroundColor = t.DefaultBackgroundColor,
                DefaultTextColor = t.DefaultTextColor,
                DefaultFontFamily = t.DefaultFontFamily,
                AvailableSections = t.AvailableSections.Select(s => new TemplateSectionResponse
                {
                    Id = s.Id,
                    Name = s.Name,
                    Required = s.Required,
                    Description = s.Description,
                    Icon = s.Icon
                }).ToList(),
                DefaultEnabledSections = t.DefaultEnabledSections,
                IsFree = t.IsFree,
                DisplayOrder = t.DisplayOrder
            })
        };

        return Ok(response);
    }
}
