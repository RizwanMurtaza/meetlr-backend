using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace Meetlr.Api.Endpoints.Auth.ForgotPassword;

[Route("api/auth")]
[ApiController]
public class ForgotPassword : ControllerBase
{
    private readonly IMediator _mediator;

    public ForgotPassword(IMediator mediator)
    {
        _mediator = mediator;
    }

    /// <summary>
    /// Initiate password reset by sending OTP to user's email
    /// </summary>
    /// <param name="request">Forgot password request containing email</param>
    /// <returns>Success message</returns>
    [HttpPost("forgot-password")]
    [ProducesResponseType(typeof(ForgotPasswordResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> Handle([FromBody] ForgotPasswordRequest request)
    {
        var command = ForgotPasswordRequest.ToCommand(request);
        var commandResponse = await _mediator.Send(command);
        var response = ForgotPasswordResponse.FromCommandResponse(commandResponse);

        if (!response.Success)
        {
            return BadRequest(response);
        }

        return Ok(response);
    }
}
