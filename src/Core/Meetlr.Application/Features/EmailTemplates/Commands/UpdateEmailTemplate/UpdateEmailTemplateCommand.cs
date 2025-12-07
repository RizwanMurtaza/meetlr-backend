using MediatR;

namespace Meetlr.Application.Features.EmailTemplates.Commands.UpdateEmailTemplate;

/// <summary>
/// Command to update an existing email template
/// Users can only update Subject and Body (not variables or template type)
/// </summary>
public record UpdateEmailTemplateCommand : IRequest<UpdateEmailTemplateCommandResponse>
{
    public Guid Id { get; init; }
    public string Subject { get; init; } = string.Empty;
    public string HtmlBody { get; init; } = string.Empty;
    public string? PlainTextBody { get; init; }
    public bool IsActive { get; init; } = true;
}
