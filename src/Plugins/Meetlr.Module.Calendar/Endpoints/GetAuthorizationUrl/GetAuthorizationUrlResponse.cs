namespace Meetlr.Module.Calendar.Endpoints.GetAuthorizationUrl;

public class GetAuthorizationUrlResponse
{
    public string AuthorizationUrl { get; init; } = string.Empty;
    public string Provider { get; init; } = string.Empty;
}
