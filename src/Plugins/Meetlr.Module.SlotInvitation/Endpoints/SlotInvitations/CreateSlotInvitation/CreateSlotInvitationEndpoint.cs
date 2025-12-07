using MediatR;
using Meetlr.Module.SlotInvitation.Application.Commands.CreateSlotInvitation;
using Meetlr.Module.SlotInvitation.Endpoints.Common;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace Meetlr.Module.SlotInvitation.Endpoints.SlotInvitations.CreateSlotInvitation;

[Route("api/slot-invitations")]
public class CreateSlotInvitationEndpoint : BaseAuthenticatedEndpoint
{
    private readonly IMediator _mediator;

    public CreateSlotInvitationEndpoint(IMediator mediator)
    {
        _mediator = mediator;
    }

    /// <summary>
    /// Create a new slot invitation and queue email notification
    /// </summary>
    [HttpPost]
    [ProducesResponseType(typeof(CreateSlotInvitationResponse), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Handle([FromBody] CreateSlotInvitationRequest request)
    {
        var command = new CreateSlotInvitationCommand
        {
            MeetlrEventId = request.MeetlrEventId,
            ContactId = request.ContactId,
            SlotStartTime = request.SlotStartTime,
            SlotEndTime = request.SlotEndTime,
            SpotsReserved = request.SpotsReserved,
            InviteeEmail = request.InviteeEmail,
            InviteeName = request.InviteeName,
            ExpirationHours = request.ExpirationHours
        };

        var commandResponse = await _mediator.Send(command);

        var response = new CreateSlotInvitationResponse
        {
            Id = commandResponse.Id,
            MeetlrEventId = commandResponse.MeetlrEventId,
            ContactId = commandResponse.ContactId,
            SlotStartTime = commandResponse.SlotStartTime,
            SlotEndTime = commandResponse.SlotEndTime,
            SpotsReserved = commandResponse.SpotsReserved,
            Token = commandResponse.Token,
            InviteeEmail = commandResponse.InviteeEmail,
            InviteeName = commandResponse.InviteeName,
            ExpiresAt = commandResponse.ExpiresAt,
            ExpirationHours = commandResponse.ExpirationHours,
            Status = commandResponse.Status,
            EmailStatus = commandResponse.EmailStatus,
            CreatedAt = commandResponse.CreatedAt
        };

        return CreatedAtAction(
            nameof(GetSlotInvitations.GetSlotInvitationsEndpoint.Handle),
            "GetSlotInvitationsEndpoint",
            new { meetlrEventId = response.MeetlrEventId },
            response);
    }
}
