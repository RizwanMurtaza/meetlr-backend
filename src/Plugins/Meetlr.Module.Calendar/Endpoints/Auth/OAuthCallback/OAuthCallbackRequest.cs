using Meetlr.Application.Features.Authentication.Commands.OAuthSignup;

namespace Meetlr.Module.Calendar.Endpoints.Auth.OAuthCallback;

public record OAuthCallbackRequest
{
    public string? Code { get; init; } // Authorization code from OAuth provider
    public string? AccessToken { get; init; } // Direct access token (deprecated, use Code instead)
    public string? RefreshToken { get; init; }
    public string Provider { get; init; } = string.Empty; // "Google" or "Microsoft"
    public int? ExpiresIn { get; init; } // Token expiry in seconds
    public string? MeetlrUsername { get; init; } // Optional custom username
    public string TimeZone { get; init; } = "UTC";
    public string? RedirectUri { get; init; } // Required when using Code

    public static OAuthSignupCommand ToCommand(
        OAuthCallbackRequest request,
        string email,
        string firstName,
        string lastName,
        string providerId,
        string? profileImageUrl,
        string accessToken,
        string? oauthRefreshToken,
        DateTime? tokenExpiry,
        string? ipAddress = null,
        string? deviceInfo = null)
    {
        return new OAuthSignupCommand
        {
            Email = email,
            FirstName = firstName,
            LastName = lastName,
            ProfileImageUrl = profileImageUrl,
            Provider = request.Provider,
            ProviderId = providerId,
            AccessToken = accessToken,
            OAuthRefreshToken = oauthRefreshToken,
            TokenExpiry = tokenExpiry,
            MeetlrUsername = request.MeetlrUsername,
            TimeZone = request.TimeZone,
            IpAddress = ipAddress,
            DeviceInfo = deviceInfo
        };
    }
}
