namespace Meetlr.Api.Endpoints.Public.CheckSlotAvailability;

public class CheckSlotAvailabilityBatchRequest
{
    public Guid MeetlrEventId { get; set; }
    public List<CheckSlotAvailabilityRequest> Slots { get; set; } = new();
}
