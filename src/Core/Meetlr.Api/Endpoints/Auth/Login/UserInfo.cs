namespace Meetlr.Api.Endpoints.Auth.Login;

public record UserInfo
{
    public Guid UserId { get; init; }
    public string Email { get; init; } = string.Empty;
    public string FirstName { get; init; } = string.Empty;
    public string LastName { get; init; } = string.Empty;
    public string? MeetlrUsername { get; init; }
    public bool IsAdmin { get; init; }
}
