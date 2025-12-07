using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace Meetlr.Api.Endpoints.Auth.ResetPassword;

[Route("api/auth")]
[ApiController]
public class ResetPassword : ControllerBase
{
    private readonly IMediator _mediator;

    public ResetPassword(IMediator mediator)
    {
        _mediator = mediator;
    }

    /// <summary>
    /// Reset password using OTP code
    /// </summary>
    /// <param name="request">Reset password request containing email, OTP and new password</param>
    /// <returns>Success message and optional auth token</returns>
    [HttpPost("reset-password")]
    [ProducesResponseType(typeof(ResetPasswordResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Handle([FromBody] ResetPasswordRequest request)
    {
        var command = ResetPasswordRequest.ToCommand(request);
        var commandResponse = await _mediator.Send(command);
        var response = ResetPasswordResponse.FromCommandResponse(commandResponse);

        if (!response.Success)
        {
            return BadRequest(response);
        }

        return Ok(response);
    }
}
