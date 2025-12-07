using MediatR;
using Meetlr.Api.Endpoints.Common;
using Microsoft.AspNetCore.Mvc;

namespace Meetlr.Api.Endpoints.SmtpConfiguration.GetAll;

[ApiController]
[Route("api/smtp-configuration")]
public class GetAllSmtpConfigurationsEndpoint : BaseAuthenticatedEndpoint
{
    private readonly IMediator _mediator;

    public GetAllSmtpConfigurationsEndpoint(IMediator mediator)
    {
        _mediator = mediator;
    }

    /// <summary>
    /// Get all SMTP configurations (user, tenant, or system level)
    /// </summary>
    [HttpGet]
    [ProducesResponseType(typeof(GetAllSmtpConfigurationsResponse), StatusCodes.Status200OK)]
    public async Task<IActionResult> Handle([FromQuery] GetAllSmtpConfigurationsRequest request)
    {
        var query = GetAllSmtpConfigurationsRequest.ToQuery(request, CurrentTenantId, CurrentUserId);
        var queryResponse = await _mediator.Send(query);
        var response = GetAllSmtpConfigurationsResponse.FromQueryResponse(queryResponse);
        return Ok(response);
    }
}
