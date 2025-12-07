namespace Meetlr.Application.Features.Authentication.Commands.Register;

/// <summary>
/// Response for register command
/// </summary>
public record RegisterCommandResponse
{
    public Guid UserId { get; init; }
    public string Email { get; init; } = string.Empty;
    public string FirstName { get; init; } = string.Empty;
    public string LastName { get; init; } = string.Empty;
    public string MeetlrUsername { get; init; } = string.Empty;
    public Guid TenantId { get; init; }
    public string Subdomain { get; init; } = string.Empty;
    public string JwtToken { get; init; } = string.Empty;
    public DateTime TokenExpiry { get; init; }
}
