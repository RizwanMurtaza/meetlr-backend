namespace Meetlr.Application.Features.SmtpConfiguration.Commands.UpdateSmtpConfiguration;

public record UpdateSmtpConfigurationCommandResponse
{
    public Guid Id { get; init; }
    public bool Success { get; init; }
    public string Message { get; init; } = string.Empty;
}
