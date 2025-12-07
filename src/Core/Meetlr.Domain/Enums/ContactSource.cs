namespace Meetlr.Domain.Enums;

/// <summary>
/// Indicates how a contact was created or added to the system
/// </summary>
public enum ContactSource
{
    /// <summary>
    /// Contact created from a booking
    /// </summary>
    Booking = 1,

    /// <summary>
    /// Contact imported from external source (CSV, etc.)
    /// </summary>
    Import = 2,

    /// <summary>
    /// Contact manually created by user
    /// </summary>
    Manual = 3,

    /// <summary>
    /// Contact created via API
    /// </summary>
    API = 4
}
