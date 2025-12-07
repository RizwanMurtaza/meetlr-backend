using MediatR;
using Meetlr.Api.Endpoints.Common;
using Meetlr.Application.Features.Plugins.Queries.ValidatePaymentPlugin;
using Meetlr.Domain.Exceptions.DomainErrors.ExceptionByArea;
using Microsoft.AspNetCore.Mvc;

namespace Meetlr.Api.Endpoints.MeetlrEvents.Create;

[Route("api/MeetlrEvents")]
public class CreateMeetlrEvent : BaseAuthenticatedEndpoint
{
    private readonly IMediator _mediator;

    public CreateMeetlrEvent(IMediator mediator)
    {
        _mediator = mediator;
    }

    /// <summary>
    /// Create a new event type
    /// </summary>
    [HttpPost]
    [ProducesResponseType(typeof(CreateMeetlrEventResponse), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> Handle([FromBody] CreateMeetlrEventRequest request)
    {
        var userId = CurrentUserId;

        // Validate payment plugin is installed if event requires payment
        if (request.RequiresPayment)
        {
            // Validate PaymentProviderType is provided
            if (string.IsNullOrEmpty(request.PaymentProviderType))
            {
                throw PluginErrors.PaymentPluginRequired();
            }

            // Verify the selected payment provider is installed and connected for this user
            var validationQuery = new ValidatePaymentPluginQuery
            {
                UserId = userId,
                PaymentProviderType = request.PaymentProviderType
            };

            var validationResult = await _mediator.Send(validationQuery);

            if (!validationResult.IsValid)
            {
                throw PluginErrors.PluginNotConnected(request.PaymentProviderType);
            }
        }

        var command = CreateMeetlrEventRequest.ToCommand(request);
        var commandResponse = await _mediator.Send(command);
        var response = CreateMeetlrEventResponse.FromCommandResponse(commandResponse);

        return CreatedAtAction(nameof(Handle), new { id = response.Id }, response);
    }
}
