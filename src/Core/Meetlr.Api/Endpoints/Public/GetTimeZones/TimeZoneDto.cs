namespace Meetlr.Api.Endpoints.Public.GetTimeZones;

public class TimeZoneDto
{
    public string Id { get; set; } = string.Empty;
    public string DisplayName { get; set; } = string.Empty;
    public string StandardName { get; set; } = string.Empty;
    public string BaseUtcOffset { get; set; } = string.Empty;
}
