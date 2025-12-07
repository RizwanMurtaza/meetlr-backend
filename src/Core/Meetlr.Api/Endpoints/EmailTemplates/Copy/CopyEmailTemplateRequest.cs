using Meetlr.Application.Features.EmailTemplates.Commands.CopyEmailTemplate;
using Meetlr.Domain.Enums;

namespace Meetlr.Api.Endpoints.EmailTemplates.Copy;

public record CopyEmailTemplateRequest
{
    public EmailTemplateType TemplateType { get; init; }
    public bool IsAdmin { get; init; }

    public static CopyEmailTemplateCommand ToCommand(CopyEmailTemplateRequest request, Guid? currentTenantId, Guid currentUserId)
    {
        return new CopyEmailTemplateCommand
        {
            TemplateType = request.TemplateType,
            TenantId = request.IsAdmin ? currentTenantId : null,
            UserId = request.IsAdmin ? null : currentUserId
        };
    }
}
