using MediatR;
using Meetlr.Module.SlotInvitation.Application.Commands.DeleteSlotInvitation;
using Meetlr.Module.SlotInvitation.Endpoints.Common;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace Meetlr.Module.SlotInvitation.Endpoints.SlotInvitations.DeleteSlotInvitation;

[Route("api/slot-invitations")]
public class DeleteSlotInvitationEndpoint : BaseAuthenticatedEndpoint
{
    private readonly IMediator _mediator;

    public DeleteSlotInvitationEndpoint(IMediator mediator)
    {
        _mediator = mediator;
    }

    /// <summary>
    /// Delete (cancel) a slot invitation. Cannot delete if already booked.
    /// </summary>
    [HttpDelete("{id:guid}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Handle([FromRoute] Guid id)
    {
        var command = new DeleteSlotInvitationCommand
        {
            Id = id
        };

        await _mediator.Send(command);

        return NoContent();
    }
}
