namespace Meetlr.Application.Features.SmtpConfiguration.Commands.TestSmtpConfiguration;

public record TestSmtpConfigurationCommandResponse
{
    public bool Success { get; init; }
    public string Message { get; init; } = string.Empty;
}
