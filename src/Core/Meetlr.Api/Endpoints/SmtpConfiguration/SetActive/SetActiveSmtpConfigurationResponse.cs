using Meetlr.Application.Features.SmtpConfiguration.Commands.SetActiveSmtpConfiguration;

namespace Meetlr.Api.Endpoints.SmtpConfiguration.SetActive;

public record SetActiveSmtpConfigurationResponse
{
    public bool Success { get; init; }
    public string Message { get; init; } = string.Empty;

    public static SetActiveSmtpConfigurationResponse FromCommandResponse(SetActiveSmtpConfigurationCommandResponse response)
    {
        return new SetActiveSmtpConfigurationResponse
        {
            Success = response.Success,
            Message = response.Message
        };
    }
}
