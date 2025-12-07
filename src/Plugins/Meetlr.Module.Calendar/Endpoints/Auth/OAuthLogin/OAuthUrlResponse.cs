namespace Meetlr.Module.Calendar.Endpoints.Auth.OAuthLogin;

public record OAuthUrlResponse
{
    public string AuthorizationUrl { get; init; } = string.Empty;
    public string State { get; init; } = string.Empty;
    public string Provider { get; init; } = string.Empty;
}
