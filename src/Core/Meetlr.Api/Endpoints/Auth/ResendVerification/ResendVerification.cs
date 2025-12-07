using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Meetlr.Api.Endpoints.Auth.ResendVerification;

[ApiController]
[Route("api/auth")]
[AllowAnonymous]
public class ResendVerification : ControllerBase
{
    private readonly IMediator _mediator;

    public ResendVerification(IMediator mediator)
    {
        _mediator = mediator;
    }

    /// <summary>
    /// Resend verification email with new OTP code
    /// </summary>
    [HttpPost("resend-verification")]
    [ProducesResponseType(typeof(ResendVerificationResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> Handle([FromBody] ResendVerificationRequest request)
    {
        var command = ResendVerificationRequest.ToCommand(request);
        var commandResponse = await _mediator.Send(command);
        var response = ResendVerificationResponse.FromCommandResponse(commandResponse);

        if (!response.Success)
        {
            return BadRequest(response);
        }

        return Ok(response);
    }
}
