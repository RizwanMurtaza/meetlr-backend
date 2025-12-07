using Meetlr.Domain.Common;
using Meetlr.Module.Billing.Domain.Enums;

namespace Meetlr.Module.Billing.Domain.Entities;

/// <summary>
/// System-defined credit costs per service type.
/// This is a global entity (same across all tenants).
/// </summary>
public class SystemCreditCost : BaseGlobalAuditableEntity
{
    /// <summary>
    /// Type of service (SMS, WhatsApp, Email)
    /// </summary>
    public ServiceType ServiceType { get; set; }

    /// <summary>
    /// Number of credits consumed per use of this service
    /// </summary>
    public int CreditCost { get; set; }

    /// <summary>
    /// Human-readable description of the service
    /// </summary>
    public string Description { get; set; } = string.Empty;

    /// <summary>
    /// Whether this service type is currently active
    /// </summary>
    public bool IsActive { get; set; } = true;
}
