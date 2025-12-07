using MediatR;
using Meetlr.Module.SlotInvitation.Application.Queries.GetSlotInvitations;
using Meetlr.Module.SlotInvitation.Endpoints.Common;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace Meetlr.Module.SlotInvitation.Endpoints.SlotInvitations.GetSlotInvitations;

[Route("api/slot-invitations")]
public class GetSlotInvitationsEndpoint : BaseAuthenticatedEndpoint
{
    private readonly IMediator _mediator;

    public GetSlotInvitationsEndpoint(IMediator mediator)
    {
        _mediator = mediator;
    }

    /// <summary>
    /// Get paginated list of slot invitations for a specific event
    /// </summary>
    [HttpGet("event/{meetlrEventId:guid}")]
    [ProducesResponseType(typeof(GetSlotInvitationsResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> Handle(
        [FromRoute] Guid meetlrEventId,
        [FromQuery] int pageNumber = 1,
        [FromQuery] int pageSize = 20,
        [FromQuery] int? status = null,
        [FromQuery] string? sortBy = "CreatedAt",
        [FromQuery] bool sortDescending = true)
    {
        var query = new GetSlotInvitationsQuery
        {
            MeetlrEventId = meetlrEventId,
            PageNumber = pageNumber,
            PageSize = pageSize,
            Status = status,
            SortBy = sortBy,
            SortDescending = sortDescending
        };

        var queryResponse = await _mediator.Send(query);

        var response = new GetSlotInvitationsResponse
        {
            Invitations = queryResponse.Invitations,
            TotalCount = queryResponse.TotalCount,
            PageNumber = queryResponse.PageNumber,
            PageSize = queryResponse.PageSize,
            TotalPages = queryResponse.TotalPages,
            HasPreviousPage = queryResponse.HasPreviousPage,
            HasNextPage = queryResponse.HasNextPage
        };

        return Ok(response);
    }
}
