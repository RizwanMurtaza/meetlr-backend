namespace Meetlr.Api.Endpoints.Public.GetBySlug;

public class HostInfo
{
    public string FirstName { get; set; } = string.Empty;
    public string LastName { get; set; } = string.Empty;
    public string? CompanyName { get; set; }
    public string? LogoUrl { get; set; }
    public string? ProfileImageUrl { get; set; }
    public string? Bio { get; set; }
}
