using Meetlr.Application.Features.SmtpConfiguration.Commands.DeleteSmtpConfiguration;

namespace Meetlr.Api.Endpoints.SmtpConfiguration.Delete;

public record DeleteSmtpConfigurationResponse
{
    public bool Success { get; init; }
    public string Message { get; init; } = string.Empty;

    public static DeleteSmtpConfigurationResponse FromCommandResponse(DeleteSmtpConfigurationCommandResponse response)
    {
        return new DeleteSmtpConfigurationResponse
        {
            Success = response.Success,
            Message = response.Message
        };
    }
}
