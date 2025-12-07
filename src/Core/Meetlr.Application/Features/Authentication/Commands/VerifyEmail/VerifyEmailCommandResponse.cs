namespace Meetlr.Application.Features.Authentication.Commands.VerifyEmail;

/// <summary>
/// Response for verify email command
/// Returns JWT token upon successful verification
/// </summary>
public record VerifyEmailCommandResponse
{
    public Guid UserId { get; init; }
    public string Email { get; init; } = string.Empty;
    public string FirstName { get; init; } = string.Empty;
    public string LastName { get; init; } = string.Empty;
    public string JwtToken { get; init; } = string.Empty;
    public DateTime TokenExpiry { get; init; }
    public bool Success { get; init; }
    public string? Message { get; init; }
}
