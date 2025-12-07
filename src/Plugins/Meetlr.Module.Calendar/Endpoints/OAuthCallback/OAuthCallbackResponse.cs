namespace Meetlr.Module.Calendar.Endpoints.OAuthCallback;

public class OAuthCallbackResponse
{
    public bool Success { get; init; }
    public string Message { get; init; } = string.Empty;
    public string Provider { get; init; } = string.Empty;
    public string? Email { get; init; }
    public bool IsPlugin { get; init; }
}
