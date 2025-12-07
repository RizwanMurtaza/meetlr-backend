namespace Meetlr.Application.Features.MeetlrEvents.Queries.GetBySlug;

public class HostDto
{
    public string FirstName { get; set; } = string.Empty;
    public string LastName { get; set; } = string.Empty;
    public string? CompanyName { get; set; }
    public string? LogoUrl { get; set; }
    public string? ProfileImageUrl { get; set; }
    public string? Bio { get; set; }
}
