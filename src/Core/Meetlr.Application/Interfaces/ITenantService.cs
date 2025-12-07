namespace Meetlr.Application.Interfaces;

/// <summary>
/// Service for accessing current tenant context
/// </summary>
public interface ITenantService
{
    Guid? TenantId { get; }
    string? Subdomain { get; }
    bool IsTenantResolved { get; }
    bool IsAdmin { get; }
}
