using MediatR;
using Meetlr.Api.Endpoints.Common;
using Meetlr.Application.Features.EmailTemplates.Queries.GetEmailTemplateById;
using Microsoft.AspNetCore.Mvc;

namespace Meetlr.Api.Endpoints.EmailTemplates.GetById;

[ApiController]
[Route("api/email-templates")]
public class GetEmailTemplateByIdEndpoint : BaseAuthenticatedEndpoint
{
    private readonly IMediator _mediator;

    public GetEmailTemplateByIdEndpoint(IMediator mediator)
    {
        _mediator = mediator;
    }

    /// <summary>
    /// Get a specific email template by ID
    /// </summary>
    [HttpGet("{id:guid}")]
    [ProducesResponseType(typeof(GetEmailTemplateByIdResponse), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Handle(Guid id)
    {
        var query = new GetEmailTemplateByIdQuery { Id = id };
        var dto = await _mediator.Send(query);
        var response = GetEmailTemplateByIdResponse.FromDto(dto);
        return Ok(response);
    }
}
