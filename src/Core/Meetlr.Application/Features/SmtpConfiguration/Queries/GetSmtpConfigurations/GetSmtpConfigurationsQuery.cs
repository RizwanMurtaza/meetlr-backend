using MediatR;

namespace Meetlr.Application.Features.SmtpConfiguration.Queries.GetSmtpConfigurations;

public record GetSmtpConfigurationsQuery : IRequest<GetSmtpConfigurationsQueryResponse>
{
    public Guid? TenantId { get; init; }
    public Guid? UserId { get; init; }
}
