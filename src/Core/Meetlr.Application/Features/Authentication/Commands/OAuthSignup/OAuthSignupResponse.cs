namespace Meetlr.Application.Features.Authentication.Commands.OAuthSignup;

public record OAuthSignupResponse
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
    public string JwtToken { get; init; } = string.Empty;
    public DateTime TokenExpiry { get; init; }
    public string RefreshToken { get; init; } = string.Empty;
    public DateTime RefreshTokenExpiry { get; init; }
}
