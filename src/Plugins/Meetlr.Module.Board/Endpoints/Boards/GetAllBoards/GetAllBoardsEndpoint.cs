using MediatR;
using Meetlr.Module.Board.Application.Queries.GetAllBoards;
using Meetlr.Module.Board.Endpoints.Common;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace Meetlr.Module.Board.Endpoints.Boards.GetAllBoards;

[Route("api/boards")]
public class GetAllBoardsEndpoint : BaseAuthenticatedEndpoint
{
    private readonly IMediator _mediator;

    public GetAllBoardsEndpoint(IMediator mediator)
    {
        _mediator = mediator;
    }

    /// <summary>
    /// Get all boards with pagination
    /// </summary>
    [HttpGet]
    [ProducesResponseType(typeof(GetAllBoardsResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> Handle(
        [FromQuery] int pageNumber = 1,
        [FromQuery] int pageSize = 20)
    {
        var query = new GetAllBoardsQuery
        {
            PageNumber = pageNumber,
            PageSize = pageSize
        };

        var queryResponse = await _mediator.Send(query);

        var response = new GetAllBoardsResponse
        {
            Boards = queryResponse.Boards.Select(b => new BoardSummaryResponse
            {
                Id = b.Id,
                Name = b.Name,
                Description = b.Description,
                Color = b.Color,
                Position = b.Position,
                CreatedAt = b.CreatedAt
            }),
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
