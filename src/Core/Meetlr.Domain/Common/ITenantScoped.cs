namespace Meetlr.Domain.Common;

/// <summary>
/// Marker interface for entities that belong to a specific tenant.
/// Entities implementing this interface will automatically have tenant-based query filters applied.
/// </summary>
public interface ITenantScoped
{
    /// <summary>
    /// The tenant ID that owns this entity.
    /// </summary>
    Guid TenantId { get; set; }
}
