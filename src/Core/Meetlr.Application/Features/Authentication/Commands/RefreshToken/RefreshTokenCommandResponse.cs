namespace Meetlr.Application.Features.Authentication.Commands.RefreshToken;

public record RefreshTokenCommandResponse
{
    public string AccessToken { get; init; } = string.Empty;
    public string RefreshToken { get; init; } = string.Empty;
    public DateTime AccessTokenExpiresAt { get; init; }
    public DateTime RefreshTokenExpiresAt { get; init; }
    public Guid UserId { get; init; }
}
