using Meetlr.Application.Features.SmtpConfiguration.Commands.UpdateSmtpConfiguration;

namespace Meetlr.Api.Endpoints.SmtpConfiguration.Update;

public record UpdateSmtpConfigurationRequest
{
    public string SmtpHost { get; init; } = string.Empty;
    public int SmtpPort { get; init; }
    public string SmtpUsername { get; init; } = string.Empty;
    public string? SmtpPassword { get; init; }
    public string FromEmail { get; init; } = string.Empty;
    public string FromName { get; init; } = string.Empty;
    public bool EnableSsl { get; init; }

    public static UpdateSmtpConfigurationCommand ToCommand(UpdateSmtpConfigurationRequest request, Guid id)
    {
        return new UpdateSmtpConfigurationCommand
        {
            Id = id,
            SmtpHost = request.SmtpHost,
            SmtpPort = request.SmtpPort,
            SmtpUsername = request.SmtpUsername,
            SmtpPassword = request.SmtpPassword,
            FromEmail = request.FromEmail,
            FromName = request.FromName,
            EnableSsl = request.EnableSsl
        };
    }
}
