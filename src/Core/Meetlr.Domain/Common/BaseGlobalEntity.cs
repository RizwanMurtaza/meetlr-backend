namespace Meetlr.Domain.Common;

/// <summary>
/// Base entity for global (non-tenant-scoped) entities like templates, system settings, etc.
/// </summary>
public abstract class BaseGlobalEntity
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime? UpdatedAt { get; set; }
    public string? CreatedBy { get; set; }
    public string? UpdatedBy { get; set; }
}

/// <summary>
/// Auditable global entity with soft delete support
/// </summary>
public abstract class BaseGlobalAuditableEntity : BaseGlobalEntity
{
    public bool IsDeleted { get; set; }
    public DateTime? DeletedAt { get; set; }
    public string? DeletedBy { get; set; }
}
