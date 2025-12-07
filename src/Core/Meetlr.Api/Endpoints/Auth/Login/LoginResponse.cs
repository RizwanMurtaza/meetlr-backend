using Meetlr.Application.Features.Authentication.Commands.Login;

namespace Meetlr.Api.Endpoints.Auth.Login;

public record LoginResponse
{
    public string Token { get; init; } = string.Empty;
    public DateTime ExpiresAt { get; init; }
    public string RefreshToken { get; init; } = string.Empty;
    public DateTime RefreshTokenExpiresAt { get; init; }
    public UserInfo User { get; init; } = null!;

    public static LoginResponse FromCommandResponse(LoginCommandResponse response)
    {
        return new LoginResponse
        {
            Token = response.Token,
            ExpiresAt = response.ExpiresAt,
            RefreshToken = response.RefreshToken,
            RefreshTokenExpiresAt = response.RefreshTokenExpiresAt,
            User = new UserInfo
            {
                UserId = response.UserId,
                Email = response.Email,
                FirstName = response.FirstName,
                LastName = response.LastName,
                MeetlrUsername = response.MeetlrUsername,
                IsAdmin = response.IsAdmin
            }
        };
    }
}
