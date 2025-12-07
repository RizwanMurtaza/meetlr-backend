namespace Meetlr.Application.Features.Tenants.Commands.CreateTenantWithAdmin;

public record CreateTenantWithAdminResponse
{
    public Guid TenantId { get; init; }
    public string TenantName { get; init; } = string.Empty;
    public string Subdomain { get; init; } = string.Empty;
    public Guid AdminUserId { get; init; }
    public Guid AdminGroupId { get; init; }
    public string AdminEmail { get; init; } = string.Empty;
    public string Message { get; init; } = string.Empty;
}
