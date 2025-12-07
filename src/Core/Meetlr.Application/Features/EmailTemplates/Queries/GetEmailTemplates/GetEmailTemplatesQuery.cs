using MediatR;

namespace Meetlr.Application.Features.EmailTemplates.Queries.GetEmailTemplates;

/// <summary>
/// Query to get all email templates for current user/tenant
/// Returns templates in hierarchical order: User → Tenant → System
/// </summary>
public record GetEmailTemplatesQuery : IRequest<GetEmailTemplatesQueryResponse>
{
    public Guid? TenantId { get; init; }
    public Guid? UserId { get; init; }
    public bool IncludeSystemTemplates { get; init; } = true;
}
