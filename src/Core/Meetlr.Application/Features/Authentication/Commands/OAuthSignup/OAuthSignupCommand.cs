using MediatR;

namespace Meetlr.Application.Features.Authentication.Commands.OAuthSignup;

/// <summary>
/// Command for OAuth-based signup (Google/Outlook) with automatic tenant creation
/// </summary>
public record OAuthSignupCommand : IRequest<OAuthSignupResponse>
{
    public string Email { get; init; } = string.Empty;
    public string FirstName { get; init; } = string.Empty;
    public string LastName { get; init; } = string.Empty;
    public string? ProfileImageUrl { get; init; }
    public string Provider { get; init; } = string.Empty; // "Google" or "Microsoft"
    public string ProviderId { get; init; } = string.Empty; // OAuth provider user ID
    public string? MeetlrUsername { get; init; } // Optional: user can provide custom username
    public string TimeZone { get; init; } = "UTC";

    // OAuth tokens (for calendar integration)
    public string AccessToken { get; init; } = string.Empty;
    public string? OAuthRefreshToken { get; init; }
    public DateTime? TokenExpiry { get; init; }

    // Client info for JWT refresh token
    public string? IpAddress { get; init; }
    public string? DeviceInfo { get; init; }
}
