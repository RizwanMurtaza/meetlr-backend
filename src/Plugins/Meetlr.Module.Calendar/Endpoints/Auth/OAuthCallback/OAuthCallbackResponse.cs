using Meetlr.Application.Features.Authentication.Commands.OAuthSignup;

namespace Meetlr.Module.Calendar.Endpoints.Auth.OAuthCallback;

public record OAuthCallbackResponse
{
    public Guid UserId { get; init; }
    public Guid TenantId { get; init; }
    public string Email { get; init; } = string.Empty;
    public string FirstName { get; init; } = string.Empty;
    public string LastName { get; init; } = string.Empty;
    public string MeetlrUsername { get; init; } = string.Empty;
    public string Subdomain { get; init; } = string.Empty;
    public string TenantUrl { get; init; } = string.Empty;
    public string BookingUrl { get; init; } = string.Empty;
    public bool IsNewUser { get; init; }
    public bool IsNewTenant { get; init; }
    public string Token { get; init; } = string.Empty;
    public DateTime TokenExpiry { get; init; }
    public string RefreshToken { get; init; } = string.Empty;
    public DateTime RefreshTokenExpiry { get; init; }
    public string Message { get; init; } = string.Empty;

    public static OAuthCallbackResponse FromCommandResponse(OAuthSignupResponse response)
    {
        var message = response.IsNewTenant
            ? $"Welcome! Your tenant '{response.Subdomain}' has been created. Access it at: {response.TenantUrl}"
            : $"Welcome back! Logged in to tenant '{response.Subdomain}'";

        return new OAuthCallbackResponse
        {
            UserId = response.UserId,
            TenantId = response.TenantId,
            Email = response.Email,
            FirstName = response.FirstName,
            LastName = response.LastName,
            MeetlrUsername = response.MeetlrUsername,
            Subdomain = response.Subdomain,
            TenantUrl = response.TenantUrl,
            BookingUrl = response.BookingUrl,
            IsNewUser = response.IsNewUser,
            IsNewTenant = response.IsNewTenant,
            Token = response.JwtToken,
            TokenExpiry = response.TokenExpiry,
            RefreshToken = response.RefreshToken,
            RefreshTokenExpiry = response.RefreshTokenExpiry,
            Message = message
        };
    }
}
