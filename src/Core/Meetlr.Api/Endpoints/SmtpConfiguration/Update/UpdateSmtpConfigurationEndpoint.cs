using MediatR;
using Meetlr.Api.Endpoints.Common;
using Microsoft.AspNetCore.Mvc;

namespace Meetlr.Api.Endpoints.SmtpConfiguration.Update;

[ApiController]
[Route("api/smtp-configuration")]
public class UpdateSmtpConfigurationEndpoint : BaseAuthenticatedEndpoint
{
    private readonly IMediator _mediator;

    public UpdateSmtpConfigurationEndpoint(IMediator mediator)
    {
        _mediator = mediator;
    }

    /// <summary>
    /// Update an SMTP configuration
    /// </summary>
    [HttpPut("{id:guid}")]
    [ProducesResponseType(typeof(UpdateSmtpConfigurationResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> Handle(Guid id, [FromBody] UpdateSmtpConfigurationRequest request)
    {
        var command = UpdateSmtpConfigurationRequest.ToCommand(request, id);
        var commandResponse = await _mediator.Send(command);
        var response = UpdateSmtpConfigurationResponse.FromCommandResponse(commandResponse);
        return Ok(response);
    }
}
