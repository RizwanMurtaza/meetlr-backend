namespace Meetlr.Domain.Exceptions.Base;

/// <summary>
/// Defines application areas for error categorization
/// </summary>
public enum ApplicationArea
{
    /// <summary>
    /// System-level errors
    /// </summary>
    System = 1,

    /// <summary>
    /// User management errors
    /// </summary>
    Users = 2,

    /// <summary>
    /// Authentication and authorization errors
    /// </summary>
    Authentication = 3,

    /// <summary>
    /// Tenant management errors
    /// </summary>
    Tenants = 4,

    /// <summary>
    /// Event type management errors
    /// </summary>
    EventTypes = 5,

    /// <summary>
    /// Booking management errors
    /// </summary>
    Bookings = 6,

    /// <summary>
    /// Availability management errors
    /// </summary>
    Availability = 7,

    /// <summary>
    /// Calendar integration errors
    /// </summary>
    CalendarIntegration = 8,

    /// <summary>
    /// Email service errors
    /// </summary>
    Email = 9,

    /// <summary>
    /// Template management errors
    /// </summary>
    Templates = 10,

    /// <summary>
    /// Plugin management errors
    /// </summary>
    Plugins = 11,

    /// <summary>
    /// Payment processing errors
    /// </summary>
    Payments = 12,

    /// <summary>
    /// Homepage/Page management errors
    /// </summary>
    Pages = 13,

    /// <summary>
    /// Single-use booking link errors
    /// </summary>
    SingleUseLinks = 14,

    /// <summary>
    /// Contact management errors
    /// </summary>
    Contacts = 15,

    /// <summary>
    /// OAuth and external provider errors
    /// </summary>
    OAuth = 16,

    /// <summary>
    /// Configuration and system setup errors
    /// </summary>
    Configuration = 17,

    /// <summary>
    /// Board/Kanban management errors
    /// </summary>
    Boards = 18,

    /// <summary>
    /// Slot invitation errors
    /// </summary>
    SlotInvitations = 19
}
