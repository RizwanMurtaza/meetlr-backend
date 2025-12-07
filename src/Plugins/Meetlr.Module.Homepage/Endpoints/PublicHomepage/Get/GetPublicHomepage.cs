using MediatR;
using Meetlr.Module.Homepage.Application.Queries.GetPublicHomepage;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace Meetlr.Module.Homepage.Endpoints.PublicHomepage.Get;

[ApiController]
[Route("api/public-homepage")]
public class GetPublicHomepage : ControllerBase
{
    private readonly IMediator _mediator;

    public GetPublicHomepage(IMediator mediator)
    {
        _mediator = mediator;
    }

    /// <summary>
    /// Gets a public homepage by username
    /// </summary>
    [HttpGet("{username}")]
    [Produces("application/json")]
    [ProducesResponseType(typeof(GetPublicHomepageResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Handle(string username)
    {
        var query = new GetPublicHomepageQuery
        {
            Username = username
        };

        var response = await _mediator.Send(query);

        if (!response.Found)
        {
            return NotFound(new { message = "Homepage not found" });
        }

        return Ok(response);
    }
}
