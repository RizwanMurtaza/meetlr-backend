using Meetlr.Domain.Common;
using Meetlr.Domain.Entities.Events;
using Meetlr.Domain.Enums;

namespace Meetlr.Domain.Entities.Emails;

/// <summary>
/// Custom email template for a specific event
/// </summary>
public class EventEmailTemplate : BaseAuditableEntity
{
    public Guid MeetlrEventId { get; set; }
    public MeetlrEvent MeetlrEvent { get; set; } = null!;
    public EmailTemplateType TemplateType { get; set; }
    public string Subject { get; set; } = string.Empty;
    public string HtmlBody { get; set; } = string.Empty;
    public string? PlainTextBody { get; set; }
    public bool IsActive { get; set; } = true;
}
