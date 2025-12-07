using Meetlr.Application.Features.Authentication.Commands.RefreshToken;

namespace Meetlr.Api.Endpoints.Auth.RefreshToken;

public record RefreshTokenResponse
{
    public string AccessToken { get; init; } = string.Empty;
    public string RefreshToken { get; init; } = string.Empty;
    public DateTime AccessTokenExpiresAt { get; init; }
    public DateTime RefreshTokenExpiresAt { get; init; }

    public static RefreshTokenResponse FromCommandResponse(RefreshTokenCommandResponse response)
    {
        return new RefreshTokenResponse
        {
            AccessToken = response.AccessToken,
            RefreshToken = response.RefreshToken,
            AccessTokenExpiresAt = response.AccessTokenExpiresAt,
            RefreshTokenExpiresAt = response.RefreshTokenExpiresAt
        };
    }
}
