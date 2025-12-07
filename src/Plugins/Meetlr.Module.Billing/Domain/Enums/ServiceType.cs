namespace Meetlr.Module.Billing.Domain.Enums;

/// <summary>
/// Type of communication service that consumes credits
/// </summary>
public enum ServiceType
{
    /// <summary>
    /// Email notification (1 credit)
    /// </summary>
    Email = 0,

    /// <summary>
    /// WhatsApp message (5 credits)
    /// </summary>
    WhatsApp = 1,

    /// <summary>
    /// SMS message (6 credits)
    /// </summary>
    SMS = 2
}
