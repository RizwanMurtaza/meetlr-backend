namespace Meetlr.Application.Features.EmailTemplates.Queries.GetEmailTemplates;

public record GetEmailTemplatesQueryResponse
{
    public List<EmailTemplateDto> Templates { get; init; } = new();
}