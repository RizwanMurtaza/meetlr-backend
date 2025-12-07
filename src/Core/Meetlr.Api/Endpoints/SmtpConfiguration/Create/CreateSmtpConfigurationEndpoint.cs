using MediatR;
using Meetlr.Api.Endpoints.Common;
using Microsoft.AspNetCore.Mvc;

namespace Meetlr.Api.Endpoints.SmtpConfiguration.Create;

[ApiController]
[Route("api/smtp-configuration")]
public class CreateSmtpConfigurationEndpoint : BaseAuthenticatedEndpoint
{
    private readonly IMediator _mediator;

    public CreateSmtpConfigurationEndpoint(IMediator mediator)
    {
        _mediator = mediator;
    }

    /// <summary>
    /// Create a new SMTP configuration
    /// </summary>
    [HttpPost]
    [ProducesResponseType(typeof(CreateSmtpConfigurationResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> Handle([FromBody] CreateSmtpConfigurationRequest request)
    {
        var command = CreateSmtpConfigurationRequest.ToCommand(request, CurrentTenantId, CurrentUserId);
        var commandResponse = await _mediator.Send(command);
        var response = CreateSmtpConfigurationResponse.FromCommandResponse(commandResponse);
        return Ok(response);
    }
}
