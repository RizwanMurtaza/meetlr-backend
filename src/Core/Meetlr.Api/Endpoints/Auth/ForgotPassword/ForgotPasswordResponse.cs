using Meetlr.Application.Features.Authentication.Commands.ForgotPassword;

namespace Meetlr.Api.Endpoints.Auth.ForgotPassword;

/// <summary>
/// Response for forgot password request
/// </summary>
public record ForgotPasswordResponse
{
    public bool Success { get; init; }
    public string Message { get; init; } = string.Empty;

    public static ForgotPasswordResponse FromCommandResponse(ForgotPasswordCommandResponse commandResponse)
    {
        return new ForgotPasswordResponse
        {
            Success = commandResponse.Success,
            Message = commandResponse.Message
        };
    }
}
