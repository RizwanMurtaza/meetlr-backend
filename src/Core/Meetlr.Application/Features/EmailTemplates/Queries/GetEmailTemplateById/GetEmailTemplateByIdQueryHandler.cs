using System.Text.Json;
using MediatR;
using Meetlr.Application.Common.Interfaces;
using Meetlr.Application.Features.EmailTemplates.Queries.GetEmailTemplates;
using Meetlr.Domain.Entities.Emails;
using Meetlr.Domain.Exceptions.Http;
using Microsoft.EntityFrameworkCore;

namespace Meetlr.Application.Features.EmailTemplates.Queries.GetEmailTemplateById;

public class GetEmailTemplateByIdQueryHandler : IRequestHandler<GetEmailTemplateByIdQuery, EmailTemplateDto>
{
    private readonly IUnitOfWork _unitOfWork;

    public GetEmailTemplateByIdQueryHandler(IUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork;
    }

    public async Task<EmailTemplateDto> Handle(GetEmailTemplateByIdQuery request, CancellationToken cancellationToken)
    {
        var template = await _unitOfWork.Repository<EmailTemplate>()
            .GetQueryable()
            .Where(t => t.Id == request.Id && !t.IsDeleted)
            .FirstOrDefaultAsync(cancellationToken);

        if (template == null)
        {
            throw NotFoundException.ForEntity("EmailTemplate", request.Id);
        }

        return new EmailTemplateDto
        {
            Id = template.Id,
            TenantId = template.TenantId,
            UserId = template.UserId,
            TemplateType = template.TemplateType,
            Subject = template.Subject,
            HtmlBody = template.HtmlBody,
            PlainTextBody = template.PlainTextBody,
            AvailableVariables = ParseAvailableVariables(template.AvailableVariablesJson),
            IsActive = template.IsActive,
            IsSystemDefault = template.IsSystemDefault,
            Level = GetTemplateLevel(template),
            CreatedAt = template.CreatedAt,
            UpdatedAt = template.UpdatedAt
        };
    }

    private List<string> ParseAvailableVariables(string json)
    {
        try
        {
            return JsonSerializer.Deserialize<List<string>>(json) ?? new List<string>();
        }
        catch
        {
            return new List<string>();
        }
    }

    private string GetTemplateLevel(EmailTemplate template)
    {
        if (template.UserId.HasValue) return "User";
        if (template.TenantId.HasValue) return "Tenant";
        return "System";
    }
}
