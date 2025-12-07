using MediatR;
using Meetlr.Api.Endpoints.Common;
using Meetlr.Application.Features.SmtpConfiguration.Commands.TestSmtpConfiguration;
using Microsoft.AspNetCore.Mvc;

namespace Meetlr.Api.Endpoints.SmtpConfiguration.Test;

[ApiController]
[Route("api/smtp-configuration")]
public class TestSmtpConfigurationEndpoint : BaseAuthenticatedEndpoint
{
    private readonly IMediator _mediator;

    public TestSmtpConfigurationEndpoint(IMediator mediator)
    {
        _mediator = mediator;
    }

    /// <summary>
    /// Test an SMTP configuration connection
    /// </summary>
    [HttpPost("{id:guid}/test")]
    [ProducesResponseType(typeof(TestSmtpConfigurationResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Handle(Guid id)
    {
        var command = new TestSmtpConfigurationCommand { Id = id };
        var commandResponse = await _mediator.Send(command);
        var response = TestSmtpConfigurationResponse.FromCommandResponse(commandResponse);
        return Ok(response);
    }
}
