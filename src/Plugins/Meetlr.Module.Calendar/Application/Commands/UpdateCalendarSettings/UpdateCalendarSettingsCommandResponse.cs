using Meetlr.Module.Calendar.Application.Queries.GetConnectedCalendars;

namespace Meetlr.Module.Calendar.Application.Commands.UpdateCalendarSettings;

public class UpdateCalendarSettingsCommandResponse
{
    public bool Success { get; set; }
    public string Message { get; set; } = string.Empty;
    public Guid? CalendarIntegrationId { get; set; }
    public CalendarIntegrationDto? Integration { get; set; }
}
