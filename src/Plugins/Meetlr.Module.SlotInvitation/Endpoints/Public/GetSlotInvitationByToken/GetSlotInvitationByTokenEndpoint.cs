using MediatR;
using Meetlr.Module.SlotInvitation.Application.Queries.GetSlotInvitationByToken;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace Meetlr.Module.SlotInvitation.Endpoints.Public.GetSlotInvitationByToken;

[ApiController]
[Route("api/public/slot-invitations")]
[AllowAnonymous]
public class GetSlotInvitationByTokenEndpoint : ControllerBase
{
    private readonly IMediator _mediator;

    public GetSlotInvitationByTokenEndpoint(IMediator mediator)
    {
        _mediator = mediator;
    }

    /// <summary>
    /// Get slot invitation details by token (public endpoint).
    /// Used by the booking page to display invitation details.
    /// </summary>
    [HttpGet("by-token/{token}")]
    [ProducesResponseType(typeof(GetSlotInvitationByTokenQueryResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Handle([FromRoute] string token)
    {
        var query = new GetSlotInvitationByTokenQuery
        {
            Token = token
        };

        var result = await _mediator.Send(query);

        if (result == null)
        {
            return NotFound(new { message = "Invitation not found or invalid" });
        }

        return Ok(result);
    }
}
