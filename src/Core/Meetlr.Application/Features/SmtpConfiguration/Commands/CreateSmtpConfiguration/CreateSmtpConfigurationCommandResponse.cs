namespace Meetlr.Application.Features.SmtpConfiguration.Commands.CreateSmtpConfiguration;

public record CreateSmtpConfigurationCommandResponse
{
    public Guid Id { get; init; }
    public bool Success { get; init; }
    public string Message { get; init; } = string.Empty;
}
