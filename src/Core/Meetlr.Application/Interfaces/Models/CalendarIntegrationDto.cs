namespace Meetlr.Application.Interfaces.Models;

/// <summary>
/// DTO for calendar integration information
/// </summary>
public class CalendarIntegrationDto
{
    public Guid Id { get; set; }
    public Guid AvailabilityScheduleId { get; set; }
    public string Provider { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public bool IsActive { get; set; }
    public bool IsPrimaryCalendar { get; set; }
    public bool CheckForConflicts { get; set; }
    public bool AddEventsToCalendar { get; set; }
    public DateTime? LastSyncedAt { get; set; }
}
