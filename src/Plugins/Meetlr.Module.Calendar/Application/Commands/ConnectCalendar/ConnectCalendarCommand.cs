using MediatR;
using Meetlr.Module.Calendar.Domain.Enums;

namespace Meetlr.Module.Calendar.Application.Commands.ConnectCalendar;

public class ConnectCalendarCommand : IRequest<ConnectCalendarResponse>
{
    public Guid ScheduleId { get; set; }
    public CalendarProvider Provider { get; set; }
    public string AccessToken { get; set; } = string.Empty;
    public string? RefreshToken { get; set; }
    public DateTime TokenExpiresAt { get; set; }
    public string? ProviderEmail { get; set; }
}
