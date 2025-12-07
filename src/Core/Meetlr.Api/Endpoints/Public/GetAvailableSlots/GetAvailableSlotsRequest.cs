namespace Meetlr.Api.Endpoints.Public.GetAvailableSlots;

public class GetAvailableSlotsRequest
{
    public string? UserSlug { get; set; }
    public DateTime StartDate { get; set; }
    public DateTime EndDate { get; set; }
    public string? TimeZone { get; set; }
}
