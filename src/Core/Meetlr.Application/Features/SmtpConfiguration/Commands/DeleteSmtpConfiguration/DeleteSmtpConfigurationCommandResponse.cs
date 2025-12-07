namespace Meetlr.Application.Features.SmtpConfiguration.Commands.DeleteSmtpConfiguration;

public record DeleteSmtpConfigurationCommandResponse
{
    public bool Success { get; init; }
    public string Message { get; init; } = string.Empty;
}
