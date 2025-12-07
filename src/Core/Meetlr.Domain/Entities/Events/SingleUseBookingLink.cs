using System.Text.Json.Serialization;
using Meetlr.Domain.Common;
using Meetlr.Domain.Entities.Users;

namespace Meetlr.Domain.Entities.Events;

/// <summary>
/// Represents a single-use booking link that can only be used once.
/// Once a booking is made using this link, it becomes inactive.
/// </summary>
public class SingleUseBookingLink : BaseAuditableEntity
{
    public Guid UserId { get; set; }
    public Guid MeetlrEventId { get; set; }
    public string Token { get; set; } = string.Empty; // Unique token for the link
    public string? Name { get; set; } // Optional name/label for the link (e.g., "For John Smith")
    public bool IsUsed { get; set; } = false;
    public DateTime? UsedAt { get; set; }
    public Guid? BookingId { get; set; } // Reference to the booking created using this link
    public DateTime? ExpiresAt { get; set; } // Optional expiration date
    public bool IsActive { get; set; } = true;

    // Navigation properties
    [JsonIgnore]
    public User User { get; set; } = null!;
    [JsonIgnore]
    public MeetlrEvent MeetlrEvent { get; set; } = null!;
    [JsonIgnore]
    public Booking? Booking { get; set; }
}
