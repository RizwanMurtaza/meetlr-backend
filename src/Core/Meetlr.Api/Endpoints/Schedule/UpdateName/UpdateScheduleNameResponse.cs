namespace Meetlr.Api.Endpoints.Schedule.UpdateName;

public class UpdateScheduleNameResponse
{
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public bool Success { get; set; }
}
