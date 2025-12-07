namespace Meetlr.Domain.Enums;

/// <summary>
/// Actions that can be audited in the system
/// </summary>
public enum AuditAction
{
    Create = 1,
    Update = 2,
    Delete = 3,
    CreateViaOAuth = 4,
    UpdateBranding = 5,
    Cancel = 6,
    Pause = 7,
    SetDefault = 8
}
