namespace Meetlr.Application.Features.Schedule.Commands.UpdateAdvancedSettings;

public class UpdateAdvancedSettingsCommandResponse
{
    public bool Success { get; set; }
    public int MaxBookingDaysInFuture { get; set; }
    public int MinBookingNoticeMinutes { get; set; }
    public int SlotIntervalMinutes { get; set; }
    public bool AutoDetectInviteeTimezone { get; set; }
}
