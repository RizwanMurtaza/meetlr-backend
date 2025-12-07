using Meetlr.Application.Features.EmailTemplates.Queries.GetEmailTemplates;

namespace Meetlr.Api.Endpoints.EmailTemplates.GetAll;

public record GetAllEmailTemplatesRequest
{
    public Guid? TenantId { get; init; }
    public Guid? UserId { get; init; }
    public bool IncludeSystemTemplates { get; init; } = true;

    public static GetEmailTemplatesQuery ToQuery(GetAllEmailTemplatesRequest request, Guid? currentTenantId, Guid currentUserId)
    {
        return new GetEmailTemplatesQuery
        {
            TenantId = request.TenantId ?? currentTenantId,
            UserId = request.UserId ?? currentUserId,
            IncludeSystemTemplates = request.IncludeSystemTemplates
        };
    }
}
