using Meetlr.Application.Features.SmtpConfiguration.Commands.TestSmtpConfiguration;

namespace Meetlr.Api.Endpoints.SmtpConfiguration.Test;

public record TestSmtpConfigurationResponse
{
    public bool Success { get; init; }
    public string Message { get; init; } = string.Empty;

    public static TestSmtpConfigurationResponse FromCommandResponse(TestSmtpConfigurationCommandResponse response)
    {
        return new TestSmtpConfigurationResponse
        {
            Success = response.Success,
            Message = response.Message
        };
    }
}
