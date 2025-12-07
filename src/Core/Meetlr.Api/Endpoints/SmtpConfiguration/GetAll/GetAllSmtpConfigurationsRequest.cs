using Meetlr.Application.Features.SmtpConfiguration.Queries.GetSmtpConfigurations;

namespace Meetlr.Api.Endpoints.SmtpConfiguration.GetAll;

public record GetAllSmtpConfigurationsRequest
{
    public Guid? TenantId { get; init; }
    public Guid? UserId { get; init; }

    public static GetSmtpConfigurationsQuery ToQuery(GetAllSmtpConfigurationsRequest request, Guid? currentTenantId, Guid currentUserId)
    {
        return new GetSmtpConfigurationsQuery
        {
            TenantId = request.TenantId ?? currentTenantId,
            UserId = request.UserId ?? currentUserId
        };
    }
}
