namespace Meetlr.Api.Endpoints.Schedule.Update;

public class UpdateAdvancedSettingsRequest
{
    public int MaxBookingDaysInFuture { get; set; } = 60;
    public int MinBookingNoticeMinutes { get; set; } = 60;
    public int SlotIntervalMinutes { get; set; } = 15;
    public bool AutoDetectInviteeTimezone { get; set; } = true;
}
