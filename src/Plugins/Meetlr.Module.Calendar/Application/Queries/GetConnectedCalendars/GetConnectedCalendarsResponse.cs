namespace Meetlr.Module.Calendar.Application.Queries.GetConnectedCalendars;

public record GetConnectedCalendarsResponse(
    List<CalendarIntegrationDto> Integrations
);
