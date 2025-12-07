using Meetlr.Domain.Common;
using Meetlr.Domain.Entities.Users;

namespace Meetlr.Domain.Entities.Tenancy;

public class Service : BaseAuditableEntity
{
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    public int DurationMinutes { get; set; } = 30;
    public decimal Price { get; set; } = 0;
    public string? Currency { get; set; } = "USD";
    public bool IsActive { get; set; } = true;
    public string? ImageUrl { get; set; }

    // Foreign Keys
    public Guid? ProviderId { get; set; } // User who provides this service

    // Navigation properties
    public Tenant Tenant { get; set; } = null!;
    public User? Provider { get; set; }
}
