namespace Meetlr.Application.Features.Profile.Queries.GetProfile;

public class GetProfileQueryResponse
{
    public Guid Id { get; set; }
    public string FirstName { get; set; } = string.Empty;
    public string LastName { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string? PhoneNumber { get; set; }
    public string MeetlrUsername { get; set; } = string.Empty;
    public string TimeZone { get; set; } = string.Empty;
    public string? CompanyName { get; set; }
    public string? WelcomeMessage { get; set; }
    public string? Language { get; set; }
    public string? DateFormat { get; set; }
    public string? TimeFormat { get; set; }
    public string? LogoUrl { get; set; }
    public string? BrandColor { get; set; }
    public string? ProfileImageUrl { get; set; }
    public string? Bio { get; set; }
}
