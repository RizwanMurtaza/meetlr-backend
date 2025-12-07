namespace Meetlr.Module.Calendar.Endpoints.Disconnect;

public class DisconnectCalendarResponse
{
    public bool Success { get; init; }
    public string Message { get; init; } = string.Empty;
    public string Provider { get; init; } = string.Empty;
}
