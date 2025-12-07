namespace Meetlr.Api.Endpoints.Public.GetTimeZones;

public class GetTimeZonesResponse
{
    public List<TimeZoneDto> TimeZones { get; set; } = new();
}
