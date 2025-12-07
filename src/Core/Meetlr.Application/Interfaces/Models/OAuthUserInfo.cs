namespace Meetlr.Application.Interfaces.Models;

public record OAuthUserInfo
{
    public string Email { get; init; } = string.Empty;
    public string FirstName { get; init; } = string.Empty;
    public string LastName { get; init; } = string.Empty;
    public string? ProfileImageUrl { get; init; }
    public string ProviderId { get; init; } = string.Empty;
    public string Provider { get; init; } = string.Empty;
}
