using Meetlr.Application.Features.Authentication.Commands.VerifyEmail;

namespace Meetlr.Api.Endpoints.Auth.VerifyEmail;

/// <summary>
/// Response for email verification
/// </summary>
public record VerifyEmailResponse
{
    public Guid UserId { get; init; }
    public string Email { get; init; } = string.Empty;
    public string FirstName { get; init; } = string.Empty;
    public string LastName { get; init; } = string.Empty;
    public string JwtToken { get; init; } = string.Empty;
    public DateTime TokenExpiry { get; init; }
    public bool Success { get; init; }
    public string? Message { get; init; }

    public static VerifyEmailResponse FromCommandResponse(VerifyEmailCommandResponse response)
    {
        return new VerifyEmailResponse
        {
            UserId = response.UserId,
            Email = response.Email,
            FirstName = response.FirstName,
            LastName = response.LastName,
            JwtToken = response.JwtToken,
            TokenExpiry = response.TokenExpiry,
            Success = response.Success,
            Message = response.Message
        };
    }
}
