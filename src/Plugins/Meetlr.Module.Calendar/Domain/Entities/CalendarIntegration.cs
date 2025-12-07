using Meetlr.Domain.Common;
using Meetlr.Module.Calendar.Domain.Enums;

namespace Meetlr.Module.Calendar.Domain.Entities;

public class CalendarIntegration : BaseAuditableEntity
{
    public Guid AvailabilityScheduleId { get; set; }
    public CalendarProvider Provider { get; set; }
    public string ProviderEmail { get; set; } = string.Empty;
    public string ProviderCalendarId { get; set; } = string.Empty;
    public string AccessToken { get; set; } = string.Empty; // Encrypted
    public string? RefreshToken { get; set; } // Encrypted
    public DateTime? TokenExpiresAt { get; set; }
    public bool IsActive { get; set; } = true;
    public bool IsPrimaryCalendar { get; set; } = false; // The calendar selected for adding new bookings
    public bool CheckForConflicts { get; set; } = true;
    public bool AddEventsToCalendar { get; set; } = false; // Only add events when explicitly selected
    public bool IncludeBuffers { get; set; } = false; // Include buffer times in this calendar
    public bool AutoSync { get; set; } = true; // Automatically sync changes from this calendar
    public DateTime? LastSyncedAt { get; set; }
}
