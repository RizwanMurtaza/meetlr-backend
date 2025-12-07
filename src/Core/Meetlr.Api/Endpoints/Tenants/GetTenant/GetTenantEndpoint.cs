using MediatR;
using Meetlr.Application.Features.Tenants.Queries.GetTenantBySubdomain;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Meetlr.Api.Endpoints.Tenants.GetTenant;

[ApiController]
[Route("api/tenants")]
[AllowAnonymous]
public class GetTenantEndpoint : ControllerBase
{
    private readonly IMediator _mediator;

    public GetTenantEndpoint(IMediator mediator)
    {
        _mediator = mediator;
    }

    /// <summary>
    /// Get tenant information by subdomain
    /// </summary>
    [HttpGet("{subdomain}")]
    [ProducesResponseType(typeof(GetTenantBySubdomainResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetBySubdomain(string subdomain, [FromQuery] string? username = null)
    {
        var query = new GetTenantBySubdomainQuery
        {
            Subdomain = subdomain,
            Username = username
        };

        var response = await _mediator.Send(query);
        return Ok(response);
    }
}
