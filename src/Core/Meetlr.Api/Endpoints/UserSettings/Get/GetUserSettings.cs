using MediatR;
using Meetlr.Api.Endpoints.Common;
using Meetlr.Application.Features.UserSettings.Queries.GetUserSettings;
using Microsoft.AspNetCore.Mvc;

namespace Meetlr.Api.Endpoints.UserSettings.Get;

[Route("api/user-settings")]
public class GetUserSettings : BaseAuthenticatedEndpoint
{
    private readonly IMediator _mediator;

    public GetUserSettings(IMediator mediator)
    {
        _mediator = mediator;
    }

    [HttpGet]
    [Produces("application/json")]
    [ProducesResponseType(typeof(UserSettingsResponseModel), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> Handle(CancellationToken cancellationToken)
    {
        var query = new GetUserSettingsQuery
        {
            UserId = CurrentUserId
        };

        var result = await _mediator.Send(query, cancellationToken);

        var response = UserSettingsResponseModel.FromQueryResponse(result);

        return Ok(response);
    }
}
