using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Meetlr.Api.Endpoints.Auth.VerifyEmail;

[ApiController]
[Route("api/auth")]
[AllowAnonymous]
public class VerifyEmail : ControllerBase
{
    private readonly IMediator _mediator;

    public VerifyEmail(IMediator mediator)
    {
        _mediator = mediator;
    }

    /// <summary>
    /// Verify user email with OTP code
    /// </summary>
    [HttpPost("verify-email")]
    [ProducesResponseType(typeof(VerifyEmailResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> Handle([FromBody] VerifyEmailRequest request)
    {
        var command = VerifyEmailRequest.ToCommand(request);
        var commandResponse = await _mediator.Send(command);
        var response = VerifyEmailResponse.FromCommandResponse(commandResponse);

        if (!response.Success)
        {
            return BadRequest(response);
        }

        return Ok(response);
    }
}
