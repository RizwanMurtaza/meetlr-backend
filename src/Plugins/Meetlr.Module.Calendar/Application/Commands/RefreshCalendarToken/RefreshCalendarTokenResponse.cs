namespace Meetlr.Module.Calendar.Application.Commands.RefreshCalendarToken;

public class RefreshCalendarTokenResponse
{
    public bool Success { get; set; }
    public string AccessToken { get; set; } = string.Empty;
    public DateTime ExpiresAt { get; set; }
}
