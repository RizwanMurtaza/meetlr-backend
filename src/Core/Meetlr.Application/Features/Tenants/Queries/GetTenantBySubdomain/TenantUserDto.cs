namespace Meetlr.Application.Features.Tenants.Queries.GetTenantBySubdomain;

public record TenantUserDto
{
    public Guid UserId { get; init; }
    public string FirstName { get; init; } = string.Empty;
    public string LastName { get; init; } = string.Empty;
    public string? MeetlrUsername { get; init; }
    public string? ProfileImageUrl { get; init; }
    public string? Bio { get; init; }
}
