using MediatR;
using Meetlr.Application.Features.EmailTemplates.Queries.GetEmailTemplates;

namespace Meetlr.Application.Features.EmailTemplates.Queries.GetEmailTemplateById;

/// <summary>
/// Query to get a specific email template by ID
/// </summary>
public record GetEmailTemplateByIdQuery : IRequest<EmailTemplateDto>
{
    public Guid Id { get; init; }
}
