using MediatR;
using Meetlr.Module.Homepage.Application.Queries.GetHomepageTemplates;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace Meetlr.Module.Homepage.Endpoints.HomepageTemplates.Get;

[ApiController]
[Route("api/homepage-templates")]
public class GetHomepageTemplates : ControllerBase
{
    private readonly IMediator _mediator;

    public GetHomepageTemplates(IMediator mediator)
    {
        _mediator = mediator;
    }

    /// <summary>
    /// Gets all available homepage templates
    /// </summary>
    [HttpGet]
    [Produces("application/json")]
    [ProducesResponseType(typeof(GetHomepageTemplatesResponse), StatusCodes.Status200OK)]
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

        var response = await _mediator.Send(query);
        return Ok(response);
    }
}
