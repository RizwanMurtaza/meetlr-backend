using Meetlr.Domain.Common;

namespace Meetlr.Domain.Entities.Tenancy;

public class Group : BaseAuditableEntity
{
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    public bool IsAdminGroup { get; set; } = false; // Admin group for tenant
    public bool IsActive { get; set; } = true;

    // Navigation properties
    public Tenant Tenant { get; set; } = null!;
    public ICollection<UserGroup> UserGroups { get; set; } = new List<UserGroup>();
}
