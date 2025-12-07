namespace Meetlr.Application.Features.Authentication.Commands.Login;

/// <summary>
/// Response for login command
/// </summary>
public record LoginCommandResponse
{
    public string Token { get; init; } = string.Empty;
    public DateTime ExpiresAt { get; init; }
    public string RefreshToken { get; init; } = string.Empty;
    public DateTime RefreshTokenExpiresAt { get; init; }
    public Guid UserId { get; init; }
    public string Email { get; init; } = string.Empty;
    public string FirstName { get; init; } = string.Empty;
    public string LastName { get; init; } = string.Empty;
    public string? MeetlrUsername { get; init; }
    public bool IsAdmin { get; init; }
}
