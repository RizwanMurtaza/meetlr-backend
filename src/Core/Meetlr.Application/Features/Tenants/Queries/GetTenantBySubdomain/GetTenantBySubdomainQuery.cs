using MediatR;

namespace Meetlr.Application.Features.Tenants.Queries.GetTenantBySubdomain;

/// <summary>
/// Query to get tenant information by subdomain
/// </summary>
public record GetTenantBySubdomainQuery : IRequest<GetTenantBySubdomainResponse>
{
    public string Subdomain { get; init; } = string.Empty;
    public string? Username { get; init; } // Optional: for tenant.mywebsite.com/username filtering
}
