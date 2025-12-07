using Meetlr.Domain.Enums;

namespace Meetlr.Application.Features.MeetlrEvents.Queries.GetEmailTemplates;

public class GetEventEmailTemplatesResponse
{
    public Guid? Id { get; set; }
    public Guid MeetlrEventId { get; set; }
    public EmailTemplateType TemplateType { get; set; }
    public string Subject { get; set; } = string.Empty;
    public string HtmlBody { get; set; } = string.Empty;
    public bool IsCustom { get; set; }
    public bool IsActive { get; set; } = true;
}
