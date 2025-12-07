using MediatR;
using Meetlr.Api.Endpoints.Common;
using Microsoft.AspNetCore.Mvc;

namespace Meetlr.Api.Endpoints.EmailTemplates.GetAll;

[ApiController]
[Route("api/email-templates")]
public class GetAllEmailTemplatesEndpoint : BaseAuthenticatedEndpoint
{
    private readonly IMediator _mediator;

    public GetAllEmailTemplatesEndpoint(IMediator mediator)
    {
        _mediator = mediator;
    }

    /// <summary>
    /// Get all email templates (user, tenant, or system level)
    /// </summary>
    [HttpGet]
    [ProducesResponseType(typeof(GetAllEmailTemplatesResponse), StatusCodes.Status200OK)]
    public async Task<IActionResult> Handle([FromQuery] GetAllEmailTemplatesRequest request)
    {
        var query = GetAllEmailTemplatesRequest.ToQuery(request, CurrentTenantId, CurrentUserId);
        var queryResponse = await _mediator.Send(query);
        var response = GetAllEmailTemplatesResponse.FromQueryResponse(queryResponse);
        return Ok(response);
    }
}
