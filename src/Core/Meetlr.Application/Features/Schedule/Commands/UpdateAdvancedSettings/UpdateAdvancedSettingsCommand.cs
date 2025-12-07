using MediatR;

namespace Meetlr.Application.Features.Schedule.Commands.UpdateAdvancedSettings;

public class UpdateAdvancedSettingsCommand : IRequest<UpdateAdvancedSettingsCommandResponse>
{
    public Guid ScheduleId { get; set; }
    public Guid UserId { get; set; }
    public int MaxBookingDaysInFuture { get; set; }
    public int MinBookingNoticeMinutes { get; set; }
    public int SlotIntervalMinutes { get; set; }
    public bool AutoDetectInviteeTimezone { get; set; }
}
