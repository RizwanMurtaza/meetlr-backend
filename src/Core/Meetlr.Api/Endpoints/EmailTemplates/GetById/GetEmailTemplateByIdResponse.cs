using Meetlr.Application.Features.EmailTemplates.Queries.GetEmailTemplates;
using Meetlr.Domain.Enums;

namespace Meetlr.Api.Endpoints.EmailTemplates.GetById;

public record GetEmailTemplateByIdResponse
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
    public string Level { get; init; } = string.Empty;

    public static GetEmailTemplateByIdResponse FromDto(EmailTemplateDto dto)
    {
        return new GetEmailTemplateByIdResponse
        {
            Id = dto.Id,
            TenantId = dto.TenantId,
            UserId = dto.UserId,
            TemplateType = dto.TemplateType,
            Subject = dto.Subject,
            HtmlBody = dto.HtmlBody,
            PlainTextBody = dto.PlainTextBody,
            AvailableVariables = dto.AvailableVariables,
            IsActive = dto.IsActive,
            Level = dto.Level
        };
    }
}
