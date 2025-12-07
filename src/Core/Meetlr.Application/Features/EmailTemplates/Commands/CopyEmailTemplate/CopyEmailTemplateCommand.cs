using MediatR;
using Meetlr.Domain.Enums;

namespace Meetlr.Application.Features.EmailTemplates.Commands.CopyEmailTemplate;

/// <summary>
/// Command to copy a template (system/tenant) to user/tenant level for customization
/// </summary>
public record CopyEmailTemplateCommand : IRequest<CopyEmailTemplateCommandResponse>
{
    public EmailTemplateType TemplateType { get; init; }
    public Guid? TenantId { get; init; }
    public Guid? UserId { get; init; }
}
