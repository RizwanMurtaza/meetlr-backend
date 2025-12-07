using System.Text.Json;
using MediatR;
using Meetlr.Application.Common.Interfaces;
using Meetlr.Domain.Entities.Emails;
using Microsoft.EntityFrameworkCore;

namespace Meetlr.Application.Features.EmailTemplates.Queries.GetEmailTemplates;

public class GetEmailTemplatesQueryHandler : IRequestHandler<GetEmailTemplatesQuery, GetEmailTemplatesQueryResponse>
{
    private readonly IUnitOfWork _unitOfWork;

    public GetEmailTemplatesQueryHandler(IUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork;
    }

    public async Task<GetEmailTemplatesQueryResponse> Handle(GetEmailTemplatesQuery request, CancellationToken cancellationToken)
    {
        var query = _unitOfWork.Repository<EmailTemplate>()
            .GetQueryable()
            .Where(t => t.IsActive && !t.IsDeleted);

        // Filter based on scope
        if (request.UserId.HasValue)
        {
            // Get user-level templates
            query = query.Where(t => t.UserId == request.UserId);
        }
        else if (request.TenantId.HasValue)
        {
            // Get tenant-level templates (excluding user-level)
            query = query.Where(t => t.TenantId == request.TenantId && t.UserId == null);
        }
        else if (request.IncludeSystemTemplates)
        {
            // Get system templates only
            query = query.Where(t => t.IsSystemDefault && t.TenantId == null && t.UserId == null);
        }

        var templates = await query
            .OrderBy(t => t.TemplateType)
            .ToListAsync(cancellationToken);

        var templateDtos = templates.Select(t => new EmailTemplateDto
        {
            Id = t.Id,
            TenantId = t.TenantId,
            UserId = t.UserId,
            TemplateType = t.TemplateType,
            Subject = t.Subject,
            HtmlBody = t.HtmlBody,
            PlainTextBody = t.PlainTextBody,
            AvailableVariables = ParseAvailableVariables(t.AvailableVariablesJson),
            IsActive = t.IsActive,
            IsSystemDefault = t.IsSystemDefault,
            Level = GetTemplateLevel(t),
            CreatedAt = t.CreatedAt,
            UpdatedAt = t.UpdatedAt
        }).ToList();

        return new GetEmailTemplatesQueryResponse
        {
            Templates = templateDtos
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
