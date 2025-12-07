using Meetlr.Domain.Enums;

namespace Meetlr.Application.Features.EmailTemplates.Queries.GetEmailTemplates;

public record EmailTemplateDto
{
    public Guid Id { get; init; }
    public Guid? TenantId { get; init; }
    public Guid? UserId { get; init; }
    public EmailTemplateType TemplateType { get; init; }
    public string Subject { get; init; } = string.Empty;
    public string HtmlBody { get; init; } = string.Empty;
    public string? PlainTextBody { get; init; }
    public List<string> AvailableVariables { get; init; } = new();
    public bool IsActive { get; init; }
    public bool IsSystemDefault { get; init; }
    public string Level { get; init; } = string.Empty; // "System", "Tenant", "User"
    public DateTime CreatedAt { get; init; }
    public DateTime? UpdatedAt { get; init; }
}