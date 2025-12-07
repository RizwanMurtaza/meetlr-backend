using MediatR;

namespace Meetlr.Application.Features.EmailTemplates.Commands.DeleteEmailTemplate;

/// <summary>
/// Command to soft delete an email template (reverts to parent level)
/// </summary>
public record DeleteEmailTemplateCommand : IRequest<DeleteEmailTemplateCommandResponse>
{
    public Guid Id { get; init; }
}
