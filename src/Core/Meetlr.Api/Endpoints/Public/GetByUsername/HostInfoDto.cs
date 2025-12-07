namespace Meetlr.Api.Endpoints.Public.GetByUsername;

public record HostInfoDto
{
    public string FirstName { get; init; } = string.Empty;
    public string LastName { get; init; } = string.Empty;
    public string? CompanyName { get; init; }
    public string? LogoUrl { get; init; }
}