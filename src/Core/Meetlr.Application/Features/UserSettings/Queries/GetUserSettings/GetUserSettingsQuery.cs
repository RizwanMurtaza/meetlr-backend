using MediatR;

namespace Meetlr.Application.Features.UserSettings.Queries.GetUserSettings;

public class GetUserSettingsQuery : IRequest<UserSettingsQueryResponse>
{
    public Guid UserId { get; set; }
}
