namespace Meetlr.Application.Features.SmtpConfiguration.Commands.SetActiveSmtpConfiguration;

public record SetActiveSmtpConfigurationCommandResponse
{
    public bool Success { get; init; }
    public string Message { get; init; } = string.Empty;
}
