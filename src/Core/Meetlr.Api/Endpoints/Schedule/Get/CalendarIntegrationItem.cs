namespace Meetlr.Api.Endpoints.Schedule.Get;

public class CalendarIntegrationItem
{
    public Guid Id { get; set; }
    public string Provider { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public bool IsActive { get; set; }
    public bool IsPrimaryCalendar { get; set; }
    public DateTime? LastSyncedAt { get; set; }
}
