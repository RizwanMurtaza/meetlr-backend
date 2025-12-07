using MediatR;

namespace Meetlr.Module.Calendar.Application.Commands.RefreshCalendarToken;

public class RefreshCalendarTokenCommand : IRequest<RefreshCalendarTokenResponse>
{
    public Guid IntegrationId { get; set; }
}
