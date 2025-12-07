using Meetlr.Domain.Enums;

namespace Meetlr.Api.Endpoints.EventEmailTemplates.Save;

public class SaveEventEmailTemplateRequest
{
    public EmailTemplateType TemplateType { get; set; }
    public string Subject { get; set; } = string.Empty;
    public string HtmlBody { get; set; } = string.Empty;
    public bool IsActive { get; set; } = true;
}
