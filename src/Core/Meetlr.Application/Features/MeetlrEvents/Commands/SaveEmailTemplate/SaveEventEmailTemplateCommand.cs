using MediatR;
using Meetlr.Domain.Enums;

namespace Meetlr.Application.Features.MeetlrEvents.Commands.SaveEmailTemplate;

public class SaveEventEmailTemplateCommand : IRequest<SaveEventEmailTemplateResponse>
{
    public Guid? Id { get; set; }
    public Guid MeetlrEventId { get; set; }
    public EmailTemplateType TemplateType { get; set; }
    public string Subject { get; set; } = string.Empty;
    public string HtmlBody { get; set; } = string.Empty;
    public bool IsActive { get; set; } = true;
}
