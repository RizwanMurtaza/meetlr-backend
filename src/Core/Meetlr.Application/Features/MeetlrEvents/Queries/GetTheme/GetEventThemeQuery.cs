using MediatR;

namespace Meetlr.Application.Features.MeetlrEvents.Queries.GetTheme;

public class GetEventThemeQuery : IRequest<GetEventThemeResponse?>
{
    public Guid MeetlrEventId { get; set; }
}
