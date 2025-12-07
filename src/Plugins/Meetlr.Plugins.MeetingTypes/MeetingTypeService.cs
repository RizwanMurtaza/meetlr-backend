using Meetlr.Application.Plugins;
using Meetlr.Application.Plugins.MeetingTypes;
using Meetlr.Application.Plugins.MeetingTypes.Models;
using Meetlr.Domain.Enums;

namespace Meetlr.Plugins.MeetingTypes;

/// <summary>
/// Meeting type service implementation - bridges Application layer interface with meeting type plugins
/// </summary>
public class MeetingTypeService : IMeetingTypeService
{
    private readonly IPluginFactory _pluginFactory;

    public MeetingTypeService(IPluginFactory pluginFactory)
    {
        _pluginFactory = pluginFactory;
    }

    public bool IsVideoLocationType(MeetingLocationType locationType)
    {
        return locationType switch
        {
            MeetingLocationType.Zoom => true,
            MeetingLocationType.GoogleMeet => true,
            MeetingLocationType.MicrosoftTeams => true,
            MeetingLocationType.JitsiMeet => true,
            MeetingLocationType.SlackHuddle => true,
            _ => false
        };
    }

    public async Task<MeetingCreationResult?> CreateMeetingAsync(
        MeetingLocationType locationType,
        MeetingCreationRequest request,
        CancellationToken cancellationToken = default)
    {
        var plugin = _pluginFactory.GetMeetingTypesPluginByLocationType(locationType);

        if (plugin == null)
        {
            return null;
        }

        // Check if plugin is available for the user
        if (!await plugin.IsAvailableForUserAsync(request.UserId, cancellationToken))
        {
            return null;
        }

        var result = await plugin.CreateMeetingAsync(new CreateMeetingRequest
        {
            Title = request.Title,
            StartTime = request.StartTime,
            DurationMinutes = request.DurationMinutes,
            UserId = request.UserId,
            EventSlug = request.EventSlug,
            BookingId = request.BookingId,
            AttendeeEmail = request.AttendeeEmail,
            AttendeeName = request.AttendeeName
        }, cancellationToken);

        return new MeetingCreationResult
        {
            JoinUrl = result.JoinUrl,
            MeetingId = result.MeetingId
        };
    }

    public async Task<bool> DeleteMeetingAsync(
        MeetingLocationType locationType,
        string meetingId,
        Guid userId,
        CancellationToken cancellationToken = default)
    {
        var plugin = _pluginFactory.GetMeetingTypesPluginByLocationType(locationType);

        if (plugin == null)
        {
            return false;
        }

        return await plugin.DeleteMeetingAsync(meetingId, userId, cancellationToken);
    }
}
