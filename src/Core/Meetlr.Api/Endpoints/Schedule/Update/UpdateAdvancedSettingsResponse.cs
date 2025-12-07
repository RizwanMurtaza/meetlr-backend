namespace Meetlr.Api.Endpoints.Schedule.Update;

public class UpdateAdvancedSettingsResponse
{
    public bool Success { get; set; }
    public int MaxBookingDaysInFuture { get; set; }
    public int MinBookingNoticeMinutes { get; set; }
    public int SlotIntervalMinutes { get; set; }
    public bool AutoDetectInviteeTimezone { get; set; }
}
