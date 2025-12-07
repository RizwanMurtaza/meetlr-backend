using Meetlr.Application.Features.Authentication.Commands.ResetPassword;

namespace Meetlr.Api.Endpoints.Auth.ResetPassword;

/// <summary>
/// Response for reset password request
/// </summary>
public record ResetPasswordResponse
{
    public bool Success { get; init; }
    public string Message { get; init; } = string.Empty;
    public string? Token { get; init; }

    public static ResetPasswordResponse FromCommandResponse(ResetPasswordCommandResponse commandResponse)
    {
        return new ResetPasswordResponse
        {
            Success = commandResponse.Success,
            Message = commandResponse.Message,
            Token = commandResponse.Token
        };
    }
}
